using Election.CRM.API.Data;
using Election.CRM.API.Features.Alertas.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Election.CRM.API.Features.Alertas;

public class AlertaService
{
    private readonly CrmContext _context;

    public AlertaService(CrmContext context)
    {
        _context = context;
    }

    public async Task<ResumenAlertasDto> GetResumenAsync()
    {
        // Contar cédulas duplicadas en Registros
        var registrosDuplicados = await _context.Registros
            .GroupBy(r => r.Cedula)
            .Where(g => g.Select(r => r.IdUser).Distinct().Count() > 1)
            .CountAsync();

        return new ResumenAlertasDto
        {
            TotalRegistrosDuplicados = registrosDuplicados
        };
    }

    public async Task<IEnumerable<AlertaRegistroDto>> GetAlertasRegistrosAsync()
    {
        // 1. Obtener los datos crudos desde la base de datos
        var registrosRaw = await _context.Registros
            .Include(r => r.IdUserNavigation)
            .GroupBy(r => r.Cedula)
            .Where(g => g.Select(r => r.IdUser).Distinct().Count() > 1)
            .Select(g => new
            {
                Cedula = g.Key,
                Nombre = g.First().Nombre,
                Registros = g.ToList()
            })
            .ToListAsync();

        // 2. Transformar en memoria
        var alertas = registrosRaw.Select(g => new AlertaRegistroDto
        {
            Cedula = g.Cedula,
            Nombre = g.Nombre,
            CantidadLideres = g.Registros.Select(r => r.IdUser).Distinct().Count(),
            Lideres = g.Registros
                .Select(r => new LiderInfoDto
                {
                    IdUser = r.IdUser ?? 0,
                    NombreLider = r.IdUserNavigation != null ? r.IdUserNavigation.Nombre : "Desconocido"
                })
                .DistinctBy(l => l.IdUser)
                .ToList()
        }).ToList();

        return alertas;
    }

    /* public async Task<IEnumerable<AlertaVotanteDto>> GetAlertasVotantesAsync()
    {
        // 1. Obtener los datos crudos desde la base de datos
        var votantesRaw = await _context.User
            .Include(v => v.IdUserNavigation)
            .Where(g => g.Select(v => v.IdUser).Distinct().Count() > 1)
            .Select(g => new
            {
                Nombre = _context.Registros
                    .Select(r => r.Nombre)
                    .FirstOrDefault() ?? "Nombre no registrado",
                Votantes = g.ToList()
            })
            .ToListAsync();

        // 2. Transformar en memoria
        var alertas = votantesRaw.Select(g => new AlertaVotanteDto
        {
            Nombre = g.Nombre,
            CantidadLideres = g.Votantes.Select(v => v.IdUser).Distinct().Count(),
            Lideres = g.Votantes
                .Select(v => new LiderInfoDto
                {
                    IdUser = v.IdUser,
                    NombreLider = v.IdUserNavigation != null ? v.IdUserNavigation.Nombre : "Desconocido"
                })
                .DistinctBy(l => l.IdUser)
                .ToList()
        }).ToList();

        return alertas;
    } */

    public async Task<IEnumerable<AlertaVotanteDto>> GetAlertasVotantesAsync()
    {
        // PASO 1: Identificar en la base de datos (SQL) las cédulas que tienen más de 1 líder distinto.
        // Esto es muy eficiente porque solo trae las claves (Cédulas), no todos los datos.
        var cedulasDuplicadas = await _context.Registros
            .GroupBy(r => r.Cedula)
            .Where(g => g.Select(r => r.IdUser).Distinct().Count() > 1)
            .Select(g => g.Key) // Solo nos quedamos con la Cédula
            .ToListAsync();

        // Optimización: Si no hay alertas, salimos inmediatamente sin hacer más consultas.
        if (!cedulasDuplicadas.Any())
        {
            return new List<AlertaVotanteDto>();
        }

        // PASO 2: Traer a memoria SOLO los registros de esas cédulas específicas, 
        // incluyendo la navegación al User (Líder) para obtener su nombre.
        var registrosConAlerta = await _context.Registros
            .Include(r => r.IdUserNavigation)
            .Where(r => cedulasDuplicadas.Contains(r.Cedula))
            .ToListAsync(); // A partir de aquí, la lista está en memoria (LINQ to Objects)

        // PASO 3: Construir los DTOs en memoria. 
        // Hacerlo en memoria evita errores de traducción de EF Core con métodos complejos como DistinctBy dentro de un Select SQL.
        var alertas = registrosConAlerta
            .GroupBy(r => r.Cedula)
            .Select(g => new AlertaVotanteDto
            {
                // Tomamos el nombre del votante del primer registro del grupo 
                // (se asume que el nombre es consistente para esa cédula)
                Nombre = g.First().Nombre, 
                
                // Contamos cuántos líderes distintos hay en este grupo
                CantidadLideres = g.Select(r => r.IdUser).Distinct().Count(),
                
                // Construimos la lista de líderes, eliminando duplicados por IdUser
                Lideres = g.Select(r => new LiderInfoDto
                {
                    IdUser = r.IdUser,
                    NombreLider = r.IdUserNavigation?.Nombre ?? "Líder desconocido"
                })
                .DistinctBy(l => l.IdUser) // DistinctBy funciona perfectamente en memoria
                .ToList()
            })
            .ToList();

        return alertas;
    }
}