using Election.CRM.API.Common.Services;
using Election.CRM.API.Common.DTOs;
using Election.CRM.API.Data;
using Election.CRM.API.Features.Ocasionale.DTOs;
using Election.CRM.API.Features.Ocasionale.Validators;
using Microsoft.EntityFrameworkCore;

namespace Election.CRM.API.Features.Ocasionale;

public class OcasionaleService
{
    private readonly CrmContext _context;
    private readonly OcasionaleValidator _validator;
    private readonly CurrentUserService _currentUserService;

    public OcasionaleService(
        CrmContext context,
        OcasionaleValidator validator,
        CurrentUserService currentUserService)
    {
        _context = context;
        _validator = validator;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<OcasionaleDto>> GetAllOcasionalesAsync()
    {
        return await _context.Ocasionales
            .Include(o => o.IdUserNavigation)
            .Include(o => o.IdZonaNavigation)
            .Select(o => new OcasionaleDto
            {
                IdOcasional = o.Id,
                Cedula = o.Cedula,
                Nombre = o.Nombre,
                IdUser = o.IdUser,
                NombreLider = o.IdUserNavigation != null ? o.IdUserNavigation.Nombre : "Desconocido",
                IdZona = o.IdZona,
                NameZona = o.IdZonaNavigation != null ? o.IdZonaNavigation.NameZona : "Sin zona",
                FechaOcasional = o.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PaginatedResponse<OcasionaleDto>> GetPaginatedOcasionalesAsync(
    PaginatedRequest request,
    int? idUser,
    int? idZona,
    string? cedula)
    {
        var query = _context.Ocasionales
            .Include(o => o.IdUserNavigation)
            .Include(o => o.IdZonaNavigation)
            .AsQueryable();

        // Filtros específicos
        if (idUser.HasValue)
            query = query.Where(o => o.IdUser == idUser.Value);
        if (idZona.HasValue)
            query = query.Where(o => o.IdZona == idZona.Value);
        if (!string.IsNullOrWhiteSpace(cedula))
            query = query.Where(o => o.Cedula.Contains(cedula));

        // Búsqueda general
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(o => o.Nombre.Contains(request.SearchTerm) ||
                                    o.Cedula.Contains(request.SearchTerm));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OcasionaleDto
            {
                IdOcasional = o.Id,
                IdUser = o.IdUser,
                NombreLider = o.IdUserNavigation != null ? o.IdUserNavigation.Nombre : null,
                IdZona = o.IdZona,
                NameZona = o.IdZonaNavigation != null ? o.IdZonaNavigation.NameZona : null,
                Cedula = o.Cedula,
                Nombre = o.Nombre,
                FechaOcasional = o.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<OcasionaleDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<OcasionaleDto> GetOcasionalByIdAsync(int id)
    {
        var ocasional = await _context.Ocasionales
            .Include(o => o.IdUserNavigation)
            .Include(o => o.IdZonaNavigation)
            .Where(o => o.Id == id)
            .Select(o => new OcasionaleDto
            {
                IdOcasional = o.Id,
                Cedula = o.Cedula,
                Nombre = o.Nombre,
                IdUser = o.IdUser,
                NombreLider = o.IdUserNavigation != null ? o.IdUserNavigation.Nombre : "Desconocido",
                IdZona = o.IdZona,
                NameZona = o.IdZonaNavigation != null ? o.IdZonaNavigation.NameZona : "Sin zona",
                FechaOcasional = o.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (ocasional == null)
            throw new KeyNotFoundException($"El ocasional con ID {id} no existe");

        return ocasional;
    }

    public async Task<OcasionaleDto> CreateOcasionalAsync(CreateOcasionaleRequest request)
    {
        // 1. Validar reglas de negocio
        await _validator.ValidateCreateAsync(request);

        // 2. Obtener el IdUser del líder autenticado
        var currentUserId = _currentUserService.GetUserId();

        // 3. Crear el ocasional
        var ocasional = new Ocasionale
        {
            Nombre = request.Nombre,
            Cedula = request.Cedula,
            IdZona = request.IdZona,
            IdUser = currentUserId,
            CreatedAt = DateTime.Now
        };

        _context.Ocasionales.Add(ocasional);
        await _context.SaveChangesAsync();

        return await GetOcasionalByIdAsync(ocasional.Id);
    }

    public async Task UpdateOcasionalAsync(int id, UpdateOcasionaleRequest request)
    {
        await _validator.ValidateUpdateAsync(id, request);

        var ocasional = await _context.Ocasionales.FindAsync(id);
        if (ocasional == null) return;

        ocasional.Nombre = request.Nombre;
        ocasional.Cedula = request.Cedula;
        ocasional.IdZona = request.IdZona;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteOcasionalAsync(int id)
    {
        await _validator.ValidateDeleteAsync(id);

        var ocasional = await _context.Ocasionales.FindAsync(id);
        if (ocasional == null) return;

        _context.Ocasionales.Remove(ocasional);
        await _context.SaveChangesAsync();
    }
}