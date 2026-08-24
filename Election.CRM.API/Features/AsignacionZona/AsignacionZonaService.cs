using Election.CRM.API.Data;
using Election.CRM.API.Features.AsignacionZona.DTOs;
using Election.CRM.API.Features.AsignacionZona.Validators;
using Microsoft.EntityFrameworkCore;

using Election.CRM.API.Common.DTOs; 

namespace Election.CRM.API.Features.AsignacionZona;

public class AsignacionZonaService
{
    private readonly CrmContext _context;
    private readonly AsignacionZonaValidator _validator;

    public AsignacionZonaService(CrmContext context, AsignacionZonaValidator validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IEnumerable<AsignacionZonaDto>> GetAllAsignacionZonaAsync()
    {
        return await _context.AsignacionZonas
            .Include(z => z.IdZonaNavigation)  
            .Include(z => z.IdUserNavigation) 
            .Select(z => new AsignacionZonaDto
            {
                IdAsig = z.Id,
                IdZona = z.IdZona,
                NameZona = z.IdZonaNavigation.NameZona,
                IdUser = z.IdUser,
                NombreUsuario = z.IdUserNavigation.Nombre,
                FechaAsignacion = z.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PaginatedResponse<AsignacionZonaDto>> GetPaginatedAsignacionesAsync(
    PaginatedRequest request,
    int? idUser,
    int? idZona)
    {
        var query = _context.AsignacionZonas
            .Include(z => z.IdUserNavigation)
            .Include(z => z.IdZonaNavigation)
            .AsQueryable();

        // Filtros específicos
        if (idUser.HasValue)
            query = query.Where(z => z.IdUser == idUser.Value);
        if (idZona.HasValue)
            query = query.Where(z => z.IdZona == idZona.Value);

        // Búsqueda general por nombre de líder o nombre de zona
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(z =>
                (z.IdUserNavigation != null && z.IdUserNavigation.Nombre.Contains(request.SearchTerm)) ||
                (z.IdZonaNavigation != null && z.IdZonaNavigation.NameZona.Contains(request.SearchTerm))
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(z => z.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(z => new AsignacionZonaDto
            {
                IdAsig = z.Id,
                IdUser = z.IdUser,
                NombreUsuario = z.IdUserNavigation != null ? z.IdUserNavigation.Nombre : null,
                IdZona = z.IdZona,
                NameZona = z.IdZonaNavigation != null ? z.IdZonaNavigation.NameZona : null,
                FechaAsignacion = z.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponse<AsignacionZonaDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<AsignacionZonaDto?> GetAsignacionZonaByIdAsync(int id)
    {
        var asignacionZona = await _context.AsignacionZonas
            .Include(z => z.IdZonaNavigation)
            .Include(z => z.IdUserNavigation)
            .Where(z => z.Id == id)
            .Select(z => new AsignacionZonaDto
            {
                IdAsig = z.Id,
                IdZona = z.IdZona,
                NameZona = z.IdZonaNavigation.NameZona,
                IdUser = z.IdUser,
                NombreUsuario = z.IdUserNavigation.Nombre,
                FechaAsignacion = z.CreatedAt
            })
            .FirstOrDefaultAsync();

            if (asignacionZona == null)
                throw new KeyNotFoundException($"La asignación de zona con ID {id} no existe");

            return asignacionZona;
    }

    public async Task<AsignacionZonaDto> CreateAsignacionZonaAsync(CreateAsignacionZonaRequest request)
    {
        await _validator.ValidateCreateAsync(request.IdZona, request.IdUser);

        var asignacionZona = new AsignacionZona
        {
            IdZona = request.IdZona,
            IdUser = request.IdUser,
            CreatedAt = DateTime.Now
        };

        _context.AsignacionZonas.Add(asignacionZona);
        await _context.SaveChangesAsync();

        return (await GetAsignacionZonaByIdAsync(asignacionZona.Id))!;
    }

    public async Task UpdateAsignacionZonaAsync(int id, UpdateAsignacionZonaRequest request)
    {
        await _validator.ValidateUpdateAsync(id, request.IdZona, request.IdUser); 

        var asignacionZona = await _context.AsignacionZonas.FindAsync(id);
        asignacionZona!.IdZona = request.IdZona;
        asignacionZona!.IdUser = request.IdUser;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsignacionZonaAsync(int id)
    {
        await _validator.ValidateDeleteAsync(id); 

        var asignacionZona = await _context.AsignacionZonas.FindAsync(id);
        _context.AsignacionZonas.Remove(asignacionZona!);
        await _context.SaveChangesAsync();
    }
}