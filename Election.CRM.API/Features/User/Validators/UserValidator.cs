using Microsoft.EntityFrameworkCore;
using Election.CRM.API.Data;
using Election.CRM.API.Common.Services;
using Election.CRM.API.Features.User.DTOs;

namespace Election.CRM.API.Features.User.Validators;

public class UserValidator
{
    private readonly CrmContext _context;
    private readonly CurrentUserService _currentUserService;

    // ID del SuperAdmin (creador del sistema) - no puede ser eliminado
    private const int SUPERADMIN_ID = 1;

    public UserValidator(CrmContext context, CurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de crear un usuario
    /// </summary>
    public async Task ValidateCreateAsync(CreateUserRequest request)
    {
        // 1. Validar permisos (solo Admin puede crear usuarios)
        _currentUserService.ValidateIsAdmin();

        // 2. Validar reglas de negocio
        await ValidateCedulaUniqueAsync(request.Cedula);
        await ValidateEmailUniqueAsync(request.Email);
        await ValidateRoleExistsAsync(request.IdRol);
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de actualizar un usuario
    /// </summary>
    public async Task ValidateUpdateAsync(int idUser, UpdateUserRequest request)
    {
        // 1. Validar permisos
        _currentUserService.ValidateIsAdmin();

        // 2. Validar existencia
        await ValidateUserExistsAsync(idUser);

        // 3. Validar unicidad (excluyendo el usuario actual)
        await ValidateCedulaUniqueForUpdateAsync(idUser, request.Cedula);
        await ValidateEmailUniqueForUpdateAsync(idUser, request.Email);

        // 4. Validar rol
        await ValidateRoleExistsAsync(request.IdRol);
    }

    /// <summary>
    /// Valida todas las reglas de negocio antes de eliminar un usuario
    /// </summary>
    public async Task ValidateDeleteAsync(int idUser)
    {
        // 1. Validar permisos
        _currentUserService.ValidateIsAdmin();

        // 2. Validar existencia
        await ValidateUserExistsAsync(idUser);

        // 3. Proteger al SuperAdmin del sistema
        await ValidateNotSuperAdmin(idUser);
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

    private async Task ValidateCedulaUniqueAsync(string cedula)
    {
        var exists = await _context.Users.AnyAsync(u => u.Cedula == cedula);
        if (exists)
            throw new InvalidOperationException("Ya existe un usuario con esa cédula");
    }

    private async Task ValidateCedulaUniqueForUpdateAsync(int idUser, string cedula)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.Cedula == cedula && u.Id != idUser);
        if (exists)
            throw new InvalidOperationException("Ya existe otro usuario con esa cédula");
    }

    private async Task ValidateEmailUniqueAsync(string email)
    {
        var exists = await _context.Users.AnyAsync(u => u.Email == email);
        if (exists)
            throw new InvalidOperationException("Ya existe un usuario con ese email");
    }

    private async Task ValidateEmailUniqueForUpdateAsync(int idUser, string email)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.Email == email && u.Id != idUser);
        if (exists)
            throw new InvalidOperationException("Ya existe otro usuario con ese email");
    }

    private async Task ValidateRoleExistsAsync(int idRol)
    {
        var exists = await _context.Roles.AnyAsync(r => r.Id == idRol);
        if (!exists)
            throw new InvalidOperationException($"El rol con ID {idRol} no existe");
    }

    private Task ValidateNotSuperAdmin(int idUser)
    {
        if (idUser == SUPERADMIN_ID)
            throw new InvalidOperationException("No se puede eliminar al SuperAdmin del sistema");

        return Task.CompletedTask;
    }
}