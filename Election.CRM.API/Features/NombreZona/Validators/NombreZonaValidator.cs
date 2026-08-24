using Election.CRM.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Election.CRM.API.Features.NombreZona.Validators;

public class NombreZonaValidator
{
    private readonly CrmContext _context;

    public NombreZonaValidator(CrmContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de crear un rol
    /// </summary>
    public async Task ValidateCreateAsync(string nameZona)
    {
        await ValidateNombreZonaNotExistsAsync(nameZona);
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de actualizar un rol
    /// </summary>
    public async Task ValidateUpdateAsync(int idZona, string nameZona)
    {
        await ValidateNombreZonaeExistsAsync(idZona);
        await ValidateNombreZonaNotExistsForUpdateAsync(idZona, nameZona);
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de eliminar un rol
    /// </summary>
    public async Task ValidateDeleteAsync(int idZona)
    {
        await ValidateNombreZonaeExistsAsync(idZona);
    }

    // ============================================
    // VALIDACIONES PRIVADAS (reutilizables)
    // ============================================

    private async Task ValidateNombreZonaNotExistsAsync(string nameZona)
    {
        var exists = await _context.NombreZonas
            .AnyAsync(r => r.NameZona.ToLower() == nameZona.ToLower());
        
        if (exists)
            throw new InvalidOperationException("Ya existe una zona con ese nombre");
    }

    private async Task ValidateNombreZonaeExistsAsync(int idZona)
    {
        var exists = await _context.NombreZonas.AnyAsync(r => r.Id == idZona);
        if (!exists)
            throw new InvalidOperationException($"El nombre de la zona con ID {idZona} no existe");
    }

    private async Task ValidateNombreZonaNotExistsForUpdateAsync(int idZona, string nameZona)
    {
        var exists = await _context.NombreZonas
            .AnyAsync(r => r.NameZona.ToLower() == nameZona.ToLower() && r.Id != idZona);
        
        if (exists)
            throw new InvalidOperationException("Ya existe otra zona con ese nombre");
    }
}