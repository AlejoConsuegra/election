using Election.CRM.API.Common.Services;
using Election.CRM.API.Data;
using Election.CRM.API.Features.Registro.DTOs;
using Election.CRM.API.Features.Registro.Validators;
using Microsoft.EntityFrameworkCore;

using Election.CRM.API.Common.DTOs;

namespace Election.CRM.API.Features.Registro;

public class RegistroService
{
    private readonly CrmContext _context;
    private readonly RegistroValidator _validator;
    private readonly CurrentUserService _currentUserService;
    private readonly RegistroExcelProcessor _excelProcessor;

    public RegistroService(
        CrmContext context, 
        RegistroValidator validator,
        CurrentUserService currentUserService,
        RegistroExcelProcessor excelProcessor)
    {
        _context = context;
        _validator = validator;
        _currentUserService = currentUserService;
        _excelProcessor = excelProcessor;
    }

    public async Task<IEnumerable<RegistroDto>> GetAllRegistrosAsync()
    {
        return await _context.Registros
            .Include(r => r.IdUserNavigation)
            .Select(r => new RegistroDto
            {
                IdRegistro = r.Id,
                IdUser = r.IdUser,
                NombreLider = r.IdUserNavigation != null ? r.IdUserNavigation.Nombre : null,
                Nombre = r.Nombre,
                Cedula = r.Cedula,
                Direccion = r.Direccion,
                Barrio = r.Barrio,
                Celular = r.Celular,
                Puesto = r.Puesto
            })
            .ToListAsync();
    }

    public async Task<PaginatedResponse<RegistroDto>> GetPaginatedRegistrosAsync(
    PaginatedRequest request, 
    int? idUser, 
    string? barrio, 
    string? cedula)
    {
        // 1. Consulta base con los JOINs necesarios
        var query = _context.Registros
            .Include(r => r.IdUserNavigation)
            .AsQueryable();

        // 2. Aplicar filtros dinámicos (solo si el parámetro tiene valor)
        if (idUser.HasValue)
        {
            query = query.Where(r => r.IdUser == idUser.Value);
        }

        if (!string.IsNullOrWhiteSpace(barrio))
        {
            query = query.Where(r => r.Barrio.Contains(barrio));
        }

        if (!string.IsNullOrWhiteSpace(cedula))
        {
            query = query.Where(r => r.Cedula.Contains(cedula));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(r => r.Nombre.Contains(request.SearchTerm) || 
                                    r.Cedula.Contains(request.SearchTerm));
        }

        // 3. Contar el total de registros ANTES de paginar (importante para el frontend)
        var totalCount = await query.CountAsync();

        // 4. Aplicar paginación y proyectar al DTO
        var items = await query
            .OrderByDescending(r => r.Id) // Ordenar por más reciente
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new RegistroDto
            {
                IdRegistro = r.Id,
                IdUser = r.IdUser,
                NombreLider = r.IdUserNavigation != null ? r.IdUserNavigation.Nombre : null,
                Nombre = r.Nombre,
                Cedula = r.Cedula,
                Direccion = r.Direccion,
                Barrio = r.Barrio,
                Celular = r.Celular,
                Puesto = r.Puesto
            })
            .ToListAsync();

        // 5. Retornar la respuesta estructurada
        return new PaginatedResponse<RegistroDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<RegistroDto> GetRegistroByIdAsync(int id)
    {
        var registro = await _context.Registros
            .Include(r => r.IdUserNavigation)
            .Where(r => r.Id == id)
            .Select(r => new RegistroDto
            {
                IdRegistro = r.Id,
                IdUser = r.IdUser,
                NombreLider = r.IdUserNavigation != null ? r.IdUserNavigation.Nombre : null,
                Nombre = r.Nombre,
                Cedula = r.Cedula,
                Direccion = r.Direccion,
                Barrio = r.Barrio,
                Celular = r.Celular,
                Puesto = r.Puesto
            })
            .FirstOrDefaultAsync();

        if (registro == null)
            throw new KeyNotFoundException($"El registro con ID {id} no existe");

        return registro;
    }

    public async Task<RegistroDto> CreateRegistroAsync(CreateRegistroRequest request)
    {
        // 1. Validar reglas de negocio
        await _validator.ValidateCreateAsync(request);

        // 2. Determinar el IdUser final (Admin usa el del request, Líder usa el suyo)
        var idUserFinal = DeterminarIdUserFinal(request.IdUser);

        // 3. Crear el registro
        var registro = new Features.Registro.Registro
        {
            IdUser = idUserFinal,
            Nombre = request.Nombre,
            Cedula = request.Cedula,
            Direccion = request.Direccion,
            Barrio = request.Barrio,
            Celular = request.Celular,
            Puesto = request.Puesto
        };

        _context.Registros.Add(registro);
        await _context.SaveChangesAsync();

        return await GetRegistroByIdAsync(registro.Id);
    }

    public async Task UpdateRegistroAsync(int id, UpdateRegistroRequest request)
    {
        // 1. Validar reglas de negocio
        await _validator.ValidateUpdateAsync(id, request);

        // 2. Obtener el registro
        var registro = await _context.Registros.FindAsync(id);
        if (registro == null) return;

        // 3. Determinar el IdUser final
        var idUserFinal = DeterminarIdUserFinal(request.IdUser);

        // 4. Actualizar campos
        registro.IdUser = idUserFinal;
        registro.Nombre = request.Nombre;
        registro.Cedula = request.Cedula;
        registro.Direccion = request.Direccion;
        registro.Barrio = request.Barrio;
        registro.Celular = request.Celular;
        registro.Puesto = request.Puesto;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteRegistroAsync(int id)
    {
        // 1. Validar reglas de negocio
        await _validator.ValidateDeleteAsync(id);

        // 2. Eliminar
        var registro = await _context.Registros.FindAsync(id);
        if (registro == null) return;

        _context.Registros.Remove(registro);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Determina el IdUser final según el rol del usuario autenticado.
    /// Admin: usa el IdUser del request (puede asignar a cualquier líder).
    /// Líder: usa su propio IdUser de la sesión.
    /// </summary>
    private int DeterminarIdUserFinal(int? idUserRequest)
    {
        var currentUserId = _currentUserService.GetUserId();
        var currentUserRole = _currentUserService.GetUserRole();

        if (currentUserRole.ToLower() == "admin")
        {
            // Admin debe enviar el IdUser en el request
            if (!idUserRequest.HasValue)
                throw new InvalidOperationException(
                    "El administrador debe especificar a qué líder se asigna el registro");
            return idUserRequest.Value;
        }
        else if (currentUserRole.ToLower() == "lider")
        {
            // Líder siempre usa su propio IdUser
            return currentUserId;
        }
        else
        {
            throw new UnauthorizedAccessException(
                "No tiene permisos para crear registros");
        }
    }

    public async Task<CargaMasivaResponse> CargaMasivaAsync(IFormFile archivo, int? idUser)
    {
        if (archivo == null || archivo.Length == 0)
            throw new InvalidOperationException($"No se envió ningún archivo");

        var extension = Path.GetExtension(archivo.FileName).ToLower();
        if (extension != ".xlsx")
            throw new InvalidOperationException($"Solo se permiten archivos Excel (.xlsx)");

        var currentUserId = _currentUserService.GetUserId();
        var currentUserRole = _currentUserService.GetUserRole();

        int idUserAsignado;
        if (currentUserRole.ToLower() == "admin")
        {
            if (!idUser.HasValue)
                throw new InvalidOperationException($"El administrador debe especificar el ID del líder");
            idUserAsignado = idUser.Value;
        }
        else
        {
            idUserAsignado = currentUserId;
        }

        using var stream = archivo.OpenReadStream();
        return await _excelProcessor.ProcesarAsync(stream, idUserAsignado);
    }
}