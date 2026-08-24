using Election.CRM.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Election.CRM.API.Features.AsignacionZona.Validators;

public class AsignacionZonaValidator
{
    private readonly CrmContext _context;

    public AsignacionZonaValidator(CrmContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de crear una asignación
    /// </summary>
    public async Task ValidateCreateAsync(int idZona, int idUser)
    {
        await ValidateUserExistsAsync(idUser);
        await ValidateZonaExistsAsync(idZona);
        await ValidateNotAlreadyAssignedAsync(idZona, idUser);
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de actualizar una asignación
    /// </summary>
    public async Task ValidateUpdateAsync(int idAsig, int idZona, int idUser)
    {
        await ValidateAsignacionExistsAsync(idAsig);
        await ValidateUserExistsAsync(idUser);
        await ValidateZonaExistsAsync(idZona);
        // Nota: No validamos duplicado porque puede ser la misma asignación actualizada
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de eliminar una asignación
    /// </summary>
    public async Task ValidateDeleteAsync(int idAsig)
    {
        await ValidateAsignacionExistsAsync(idAsig);
    }

    // ============================================
    // VALIDACIONES PRIVADAS (reutilizables)
    // ============================================

    private async Task ValidateUserExistsAsync(int idUser)
    {
        var exists = await _context.Users.AnyAsync(u => u.Id == idUser);
        if (!exists)
            throw new InvalidOperationException($"El usuario con ID {idUser} no existe");
    }

    private async Task ValidateZonaExistsAsync(int idZona)
    {
        var exists = await _context.NombreZonas.AnyAsync(nz => nz.Id == idZona);
        if (!exists)
            throw new InvalidOperationException($"La zona con ID {idZona} no existe");
    }

    private async Task ValidateNotAlreadyAssignedAsync(int idZona, int idUser)
    {
        var exists = await _context.AsignacionZonas
            .AnyAsync(z => z.IdZona == idZona && z.IdUser == idUser);
        if (exists)
            throw new InvalidOperationException("Esta zona ya está asignada a este usuario");
    }

    private async Task ValidateAsignacionExistsAsync(int idAsig)
    {
        var exists = await _context.AsignacionZonas.AnyAsync(z => z.Id == idAsig);
        if (!exists)
            throw new InvalidOperationException($"La asignación con ID {idAsig} no existe");
    }
}