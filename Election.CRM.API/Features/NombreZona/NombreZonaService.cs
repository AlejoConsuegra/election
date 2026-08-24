using Election.CRM.API.Data;
using Election.CRM.API.Features.NombreZona.DTOs;
using Election.CRM.API.Features.NombreZona.Validators;
using Microsoft.EntityFrameworkCore;

namespace Election.CRM.API.Features.NombreZona;

public class NombreZonaService
{
    private readonly CrmContext _context;
    private readonly NombreZonaValidator _validator;

    public NombreZonaService(CrmContext context, NombreZonaValidator validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IEnumerable<NombreZonaDto>> GetAllNombreZonasAsync()
    {
        return await _context.NombreZonas
            .Select(nz => new NombreZonaDto
            {
                IdZona = nz.Id,
                NameZona = nz.NameZona
            })
            .ToListAsync();
    }

    public async Task<NombreZonaDto> GetNombreZonaByIdAsync(int id)
    {
        var nombreZona = await _context.NombreZonas
            .Where(nz => nz.Id == id)
            .Select(nz => new NombreZonaDto
            {
                IdZona = nz.Id,
                NameZona = nz.NameZona
            })
            .FirstOrDefaultAsync();

        if (nombreZona == null)
            throw new KeyNotFoundException($"La zona con ID {id} no existe");

        return nombreZona;
    }

    public async Task<NombreZonaDto> CreateNombreZonaAsync(CreateNombreZonaRequest request)
    {
        await _validator.ValidateCreateAsync(request.NameZona);

        var nombreZona = new NombreZona
        {
            NameZona = request.NameZona
        };

        _context.NombreZonas.Add(nombreZona);
        await _context.SaveChangesAsync();

        return (await GetNombreZonaByIdAsync(nombreZona.Id))!;
    }

    public async Task UpdateNombreZonaAsync(int id, UpdateNombreZonaRequest request)
    {
        await _validator.ValidateUpdateAsync(id, request.NameZona);

        var nombreZona = await _context.NombreZonas.FindAsync(id);
        nombreZona!.NameZona = request.NameZona;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteNombreZonaAsync(int id)
    {
        await _validator.ValidateDeleteAsync(id);

        var nombreZona = await _context.NombreZonas.FindAsync(id);
        _context.NombreZonas.Remove(nombreZona!);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<NombreZonaDto>> SearchByNameAsync(string name)
    {
        return await _context.NombreZonas
            .Where(nz => nz.NameZona.ToLower().Contains(name.ToLower()))
            .OrderBy(nz => nz.NameZona)  
            .Select(nz => new NombreZonaDto
            {
                IdZona = nz.Id,
                NameZona = nz.NameZona
            })
            .ToListAsync();
    }
}