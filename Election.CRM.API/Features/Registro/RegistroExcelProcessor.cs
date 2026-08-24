using Election.CRM.API.Common.Services;
using Election.CRM.API.Data;
using Election.CRM.API.Features.Registro.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Election.CRM.API.Features.Registro;

public class RegistroExcelProcessor
{
    private readonly CrmContext _context;
    private readonly ExcelService _excelService;

    public RegistroExcelProcessor(CrmContext context, ExcelService excelService)
    {
        _context = context;
        _excelService = excelService;
    }

    public async Task<CargaMasivaResponse> ProcesarAsync(Stream archivoExcel, int idUserAsignado)
    {
        var response = new CargaMasivaResponse();
        var filas = _excelService.LeerExcel(archivoExcel);

        response.TotalFilas = filas.Count;

        foreach (var fila in filas)
        {
            var numeroFila = filas.IndexOf(fila) + 2; // +2 porque saltamos encabezado y es base 1
            
            try
            {
                var cedula = fila.GetValueOrDefault("Cédula", "").Trim();
                var nombre = fila.GetValueOrDefault("Nombre", "").Trim();
                var direccion = fila.GetValueOrDefault("Dirección", "").Trim();
                var barrio = fila.GetValueOrDefault("Barrio", "").Trim();
                var celular = fila.GetValueOrDefault("Celular", "").Trim();
                var puesto = fila.GetValueOrDefault("Puesto", "").Trim();

                // Validar cédula
                if (string.IsNullOrWhiteSpace(cedula))
                {
                    response.Errores.Add(new ErrorFila
                    {
                        NumeroFila = numeroFila,
                        Cedula = cedula,
                        Motivo = "Cédula vacía"
                    });
                    response.RegistrosFallidos++;
                    continue;
                }

                // Validar nombre
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    response.Errores.Add(new ErrorFila
                    {
                        NumeroFila = numeroFila,
                        Cedula = cedula,
                        Motivo = "Nombre vacío"
                    });
                    response.RegistrosFallidos++;
                    continue;
                }

                // Verificar duplicado para este líder
                var existeParaLider = await _context.Registros
                    .AnyAsync(r => r.Cedula == cedula && r.IdUser == idUserAsignado);

                if (existeParaLider)
                {
                    response.Duplicados++;
                    continue;
                }

                // Crear registro
                var registro = new Registro
                {
                    Cedula = cedula,
                    Nombre = nombre,
                    Direccion = direccion,
                    Barrio = barrio,
                    Celular = celular,
                    Puesto = puesto,
                    IdUser = idUserAsignado
                };

                _context.Registros.Add(registro);
                response.RegistrosExitosos++;
            }
            catch (Exception ex)
            {
                response.Errores.Add(new ErrorFila
                {
                    NumeroFila = numeroFila,
                    Cedula = fila.GetValueOrDefault("Cédula", ""),
                    Motivo = $"Error al procesar: {ex.Message}"
                });
                response.RegistrosFallidos++;
            }
        }

        // Guardar todos los registros exitosos
        if (response.RegistrosExitosos > 0)
        {
            await _context.SaveChangesAsync();
        }

        return response;
    }
}