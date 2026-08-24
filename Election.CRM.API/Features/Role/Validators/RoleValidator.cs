using Election.CRM.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Election.CRM.API.Features.Role.Validators;

public class RoleValidator
{
    private readonly CrmContext _context;

    public RoleValidator(CrmContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de crear un rol
    /// </summary>
    public async Task ValidateCreateAsync(string nameRol)
    {
        await ValidateNameRolNotExistsAsync(nameRol);
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de actualizar un rol
    /// </summary>
    public async Task ValidateUpdateAsync(int idRol, string nameRol)
    {
        await ValidateRoleExistsAsync(idRol);
        await ValidateNotSystemRoleAsync(idRol);
        await ValidateNameRolNotExistsForUpdateAsync(idRol, nameRol);
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de eliminar un rol
    /// </summary>
    public async Task ValidateDeleteAsync(int idRol)
    {
        await ValidateRoleExistsAsync(idRol);
        await ValidateNotSystemRoleAsync(idRol);
    }

    // ============================================
    // VALIDACIONES PRIVADAS (reutilizables)
    // ============================================

    private async Task ValidateRoleExistsAsync(int idRol)
    {
        var exists = await _context.Roles.AnyAsync(r => r.Id == idRol);
        if (!exists)
            throw new InvalidOperationException($"El rol con ID {idRol} no existe");
    }

    private async Task ValidateNameRolNotExistsAsync(string nameRol)
    {
        var exists = await _context.Roles
            .AnyAsync(r => r.NameRol.ToLower() == nameRol.ToLower());
        
        if (exists)
            throw new InvalidOperationException("Ya existe un rol con ese nombre");
    }

    private async Task ValidateNameRolNotExistsForUpdateAsync(int idRol, string nameRol)
    {
        var exists = await _context.Roles
            .AnyAsync(r => r.NameRol.ToLower() == nameRol.ToLower() && r.Id != idRol);
        
        if (exists)
            throw new InvalidOperationException("Ya existe otro rol con ese nombre");
    }

    private Task ValidateNotSystemRoleAsync(int idRol)
    {
        if (idRol == 1 || idRol == 2)
            throw new InvalidOperationException("No se puede modificar ni eliminar roles del sistema (Admin o Lider)");
        
        return Task.CompletedTask; 
    }
}