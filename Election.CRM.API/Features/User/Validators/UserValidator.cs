using Microsoft.EntityFrameworkCore;
using Election.CRM.API.Data;
using Election.CRM.API.Common.Services;
using Election.CRM.API.Features.User.DTOs;

namespace Election.CRM.API.Features.User.Validators;

public class UserValidator
{
    private readonly CrmContext _context;
    private readonly CurrentUserService _currentUserService;
    private const int SUPERADMIN_ID = 1;

    public UserValidator(CrmContext context, CurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public virtual Task ValidateIsAdminAsync()
    {
        _currentUserService.ValidateIsAdmin();
        return Task.CompletedTask; 
    }

    public async Task ValidateCreateAsync(CreateUserRequest request)
    {
        _currentUserService.ValidateIsAdmin();

        // DEFENSA EN PROFUNDIDAD: Bloquear creación de Admins desde la API
        if (request.IdRol == 1)
        {
            throw new InvalidOperationException("No se permite la creación de usuarios con rol Admin a través de esta API.");
        }

        await ValidateCedulaUniqueAsync(request.Cedula);
        await ValidateEmailUniqueAsync(request.Email);
        await ValidateRoleExistsAsync(request.IdRol);
    }

    public async Task ValidateUpdateAsync(int idUser, UpdateUserRequest request)
    {
        _currentUserService.ValidateIsAdmin();

        // DEFENSA EN PROFUNDIDAD: Bloquear asignación de rol Admin
        if (request.IdRol == 1)
        {
            throw new InvalidOperationException("No se permite asignar el rol Admin a través de esta API.");
        }

        await ValidateUserExistsAsync(idUser);
        await ValidateCedulaUniqueForUpdateAsync(idUser, request.Cedula);
        await ValidateEmailUniqueForUpdateAsync(idUser, request.Email);
        await ValidateRoleExistsAsync(request.IdRol);
    }

    public async Task ValidateDeleteAsync(int idUser)
    {
        _currentUserService.ValidateIsAdmin();
        await ValidateUserExistsAsync(idUser);
        await ValidateNotSuperAdmin(idUser);
    }

    // ============================================
    // VALIDACIONES PRIVADAS (se mantienen igual)
    // ============================================
    private async Task ValidateUserExistsAsync(int idUser)
    {
        var exists = await _context.Users.AnyAsync(u => u.Id == idUser);
        if (!exists) throw new InvalidOperationException($"El usuario con ID {idUser} no existe");
    }

    private async Task ValidateCedulaUniqueAsync(string cedula)
    {
        var exists = await _context.Users.AnyAsync(u => u.Cedula == cedula);
        if (exists) throw new InvalidOperationException("Ya existe un usuario con esa cédula");
    }

    private async Task ValidateCedulaUniqueForUpdateAsync(int idUser, string cedula)
    {
        var exists = await _context.Users.AnyAsync(u => u.Cedula == cedula && u.Id != idUser);
        if (exists) throw new InvalidOperationException("Ya existe otro usuario con esa cédula");
    }

    private async Task ValidateEmailUniqueAsync(string email)
    {
        var exists = await _context.Users.AnyAsync(u => u.Email == email);
        if (exists) throw new InvalidOperationException("Ya existe un usuario con ese email");
    }

    private async Task ValidateEmailUniqueForUpdateAsync(int idUser, string email)
    {
        var exists = await _context.Users.AnyAsync(u => u.Email == email && u.Id != idUser);
        if (exists) throw new InvalidOperationException("Ya existe otro usuario con ese email");
    }

    private async Task ValidateRoleExistsAsync(int idRol)
    {
        var exists = await _context.Roles.AnyAsync(r => r.Id == idRol);
        if (!exists) throw new InvalidOperationException($"El rol con ID {idRol} no existe");
    }

    private Task ValidateNotSuperAdmin(int idUser)
    {
        if (idUser == SUPERADMIN_ID)
            throw new InvalidOperationException("No se puede eliminar al SuperAdmin del sistema");
        return Task.CompletedTask;
    }
}