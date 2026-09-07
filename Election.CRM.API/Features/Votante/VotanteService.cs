using Microsoft.EntityFrameworkCore;
using Election.CRM.API.Common.DTOs;
using Election.CRM.API.Common.Services;
using Election.CRM.API.Data;

using Election.CRM.API.Features.Votante.DTOs;
using Election.CRM.API.Features.Votante.Validators;

namespace Election.CRM.API.Features.Votante;

public class VotanteService
{
    private readonly CrmContext _context;
    private readonly VotanteValidator _validator;
    private readonly CurrentUserService _currentUserService;

    public VotanteService(
        CrmContext context, 
        VotanteValidator validator,
        CurrentUserService currentUserService)
    {
        _context = context;
        _validator = validator;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResponse<VotanteDto>> GetPaginatedVotantesAsync(
    PaginatedRequest request,
    int? idUser,
    string? barrio)
    {
        var query = _context.Votantes
            .Include(v => v.IdRegistroNavigation)
                .ThenInclude(r => r.IdUserNavigation!)
                    .ThenInclude(u => u.AsignacionZonas)
                        .ThenInclude(az => az.IdZonaNavigation)
            .AsQueryable();

        if (idUser.HasValue)
            query = query.Where(v => v.IdRegistroNavigation.IdUser == idUser.Value);
        
        if (!string.IsNullOrWhiteSpace(barrio))
            query = query.Where(v => v.IdRegistroNavigation.Barrio != null && v.IdRegistroNavigation.Barrio.Contains(barrio));

        // Búsqueda general
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(v => v.IdRegistroNavigation.Nombre.Contains(request.SearchTerm) ||
                                    v.IdRegistroNavigation.IdUserNavigation!.Nombre.Contains(request.SearchTerm));
        }

        // 3. Contar total
        var totalCount = await query.CountAsync();

        // 4. Paginar y ordenar
        var items = await query
            .OrderByDescending(v => v.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(v => new VotanteDto
            {
                // Datos propios del Votante
                Id = v.Id,
                IdRegistro = v.IdRegistro,
                
                // AQUÍ conectamos el 'CreatedAt' de la entidad con el 'FechaVotante' del DTO
                FechaVotante = v.CreatedAt, 
                
                // Datos del Votante (vienen de la entidad Registro)
                NombreVotante = v.IdRegistroNavigation.Nombre,
                Cedula = v.IdRegistroNavigation.Cedula,
                Direccion = v.IdRegistroNavigation.Direccion,
                Barrio = v.IdRegistroNavigation.Barrio,
                Celular = v.IdRegistroNavigation.Celular,
                Puesto = v.IdRegistroNavigation.Puesto,
                
                // Datos del Líder (vienen de la entidad User)
                NombreLider = v.IdRegistroNavigation.IdUserNavigation!.Nombre,
                
                // Datos de la Zona
                // NOTA IMPORTANTE: Un líder (User) puede tener MÚLTIPLES zonas asignadas (1:N).
                // Como tu DTO espera un solo string (NameZona), tomamos la primera zona asignada 
                // usando .FirstOrDefault(). Si no tiene zonas, asignamos un valor por defecto.
                NameZona = v.IdRegistroNavigation.IdUserNavigation!.AsignacionZonas
                            .Select(az => az.IdZonaNavigation.NameZona)
                            .FirstOrDefault() ?? "Sin zona asignada"
            })
            .ToListAsync();

        return new PaginatedResponse<VotanteDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<VotanteDto> GetVotanteByIdAsync(int id)
    {
        var votante = await _context.Votantes
            .Include(v => v.IdRegistroNavigation)
                .ThenInclude(r => r.IdUserNavigation!)
                    .ThenInclude(u => u.AsignacionZonas)
                        .ThenInclude(az => az.IdZonaNavigation)
            .Where(v => v.Id == id)
            .Select(v => new VotanteDto
            {
                 // Datos propios del Votante
                Id = v.Id,
                IdRegistro = v.IdRegistro,
                
                // AQUÍ conectamos el 'CreatedAt' de la entidad con el 'FechaVotante' del DTO
                FechaVotante = v.CreatedAt, 
                
                // Datos del Votante (vienen de la entidad Registro)
                NombreVotante = v.IdRegistroNavigation.Nombre,
                Cedula = v.IdRegistroNavigation.Cedula,
                Direccion = v.IdRegistroNavigation.Direccion,
                Barrio = v.IdRegistroNavigation.Barrio,
                Celular = v.IdRegistroNavigation.Celular,
                Puesto = v.IdRegistroNavigation.Puesto,
                
                // Datos del Líder (vienen de la entidad User)
                NombreLider = v.IdRegistroNavigation.IdUserNavigation!.Nombre,
                
                // Datos de la Zona
                // NOTA IMPORTANTE: Un líder (User) puede tener MÚLTIPLES zonas asignadas (1:N).
                // Como tu DTO espera un solo string (NameZona), tomamos la primera zona asignada 
                // usando .FirstOrDefault(). Si no tiene zonas, asignamos un valor por defecto.
                NameZona = v.IdRegistroNavigation.IdUserNavigation.AsignacionZonas
                            .Select(az => az.IdZonaNavigation.NameZona)
                            .FirstOrDefault() ?? "Sin zona asignada"
            })
            .FirstOrDefaultAsync();

        if (votante == null)
            throw new KeyNotFoundException($"El votante con ID {id} no existe");

        return votante;
    }

    public async Task<VotanteDto> CreateVotanteAsync(CreateVotanteRequest request, string ipAddress)
    {
        // 1. Obtener el Id del líder autenticado
        var currentUserId = _currentUserService.GetUserId();
        
        var registro = await _context.Registros
            .Include(r => r.IdUserNavigation!)
                .ThenInclude(u => u.AsignacionZonas)
                    .ThenInclude(az => az.IdZonaNavigation)
            .FirstOrDefaultAsync(r => r.Cedula == request.Cedula && r.IdUser == currentUserId);

        if (registro == null)
        {
            throw new InvalidOperationException("La cédula no corresponde a un votante registrado en su censo.");
        }

        // 3. Validar reglas de negocio específicas
        await _validator.ValidateCreateAsync(registro.Id, currentUserId);
        
        var votante = new Votante
        {
            IdRegistro = registro.Id,
            IpAddress = NormalizeIpAddress(ipAddress),
            CreatedAt = DateTime.UtcNow
        };

        _context.Votantes.Add(votante);
        await _context.SaveChangesAsync();
        
        Console.WriteLine($"[DEBUG] Votante guardado con éxito. ID: {votante.Id}, IP: {ipAddress}");

        // 4. RETORNO DIRECTO (Evitamos llamar a GetVotanteByIdAsync)
        return new VotanteDto
        {
            Id = votante.Id,
            IdRegistro = registro.Id,
            FechaVotante = votante.CreatedAt,
            NombreVotante = registro.Nombre,
            Cedula = registro.Cedula,
            Direccion = registro.Direccion,
            Barrio = registro.Barrio,
            Celular = registro.Celular,
            Puesto = registro.Puesto,
            NombreLider = registro.IdUserNavigation?.Nombre ?? "Líder desconocido",
            NameZona = registro.IdUserNavigation?.AsignacionZonas?
                        .Select(az => az.IdZonaNavigation?.NameZona)
                        .FirstOrDefault() ?? "Sin zona asignada"
        };
    }

    /// <summary>
    /// Normaliza direcciones IP IPv6 mapeadas a IPv4 (ej: ::ffff:172.18.0.1 → 172.18.0.1)
    /// </summary>
    private static string NormalizeIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return "unknown";

        // Si es IPv6 mapeada a IPv4, extraer solo la parte IPv4
        if (ipAddress.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase))
        {
            return ipAddress.Substring(7); // Remover "::ffff:"
        }

        return ipAddress;
    }
}