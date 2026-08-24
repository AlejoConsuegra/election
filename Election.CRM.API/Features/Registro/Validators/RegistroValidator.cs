using Election.CRM.API.Common.Services;
using Election.CRM.API.Data;
using Election.CRM.API.Features.Registro.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Election.CRM.API.Features.Registro.Validators;

public class RegistroValidator
{
    private readonly CrmContext _context;
    private readonly CurrentUserService _currentUserService;

    public RegistroValidator(CrmContext context, CurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task ValidateCreateAsync(CreateRegistroRequest request)
    {
        var idUserAsignado = DeterminarIdUserFinal(request.IdUser);
        await ValidateUserExistsAsync(idUserAsignado);
        await ValidateCedulaNotExistsForLeaderAsync(request.Cedula, idUserAsignado);
    }

    public async Task ValidateUpdateAsync(int idRegistro, UpdateRegistroRequest request)
    {
        await ValidateRegistroExistsAsync(idRegistro);
        var idUserAsignado = DeterminarIdUserFinal(request.IdUser);
        await ValidateUserExistsAsync(idUserAsignado);
        await ValidateCedulaNotExistsForLeaderForUpdateAsync(idRegistro, request.Cedula, idUserAsignado);
    }

    public async Task ValidateDeleteAsync(int idRegistro)
    {
        await ValidateRegistroExistsAsync(idRegistro);
    }

    // ============================================
    // VALIDACIONES PRIVADAS
    // ============================================

    private int DeterminarIdUserFinal(int? idUserRequest)
    {
        var currentUserId = _currentUserService.GetUserId();
        var currentUserRole = _currentUserService.GetUserRole();

        if (currentUserRole.ToLower() == "admin")
        {
            if (!idUserRequest.HasValue)
                throw new InvalidOperationException(
                    "El administrador debe especificar a qué líder se asigna el registro");
            return idUserRequest.Value;
        }
        else if (currentUserRole.ToLower() == "lider")
        {
            if (idUserRequest.HasValue && idUserRequest.Value != currentUserId)
                throw new UnauthorizedAccessException(
                    "Un líder solo puede registrar votantes bajo su propio usuario");
            return currentUserId;
        }
        else
        {
            throw new UnauthorizedAccessException(
                "No tiene permisos para crear registros");
        }
    }

    private async Task ValidateRegistroExistsAsync(int idRegistro)
    {
        var exists = await _context.Registros.AnyAsync(r => r.Id == idRegistro);
        if (!exists)
            throw new KeyNotFoundException($"El registro con ID {idRegistro} no existe");
    }

    private async Task ValidateUserExistsAsync(int idUser)
    {
        var exists = await _context.Users.AnyAsync(u => u.Id == idUser);
        if (!exists)
            throw new InvalidOperationException($"El líder con ID {idUser} no existe");
    }

    private async Task ValidateCedulaNotExistsForLeaderAsync(string cedula, int idUser)
    {
        var exists = await _context.Registros
            .AnyAsync(r => r.Cedula == cedula && r.IdUser == idUser);
        if (exists)
            throw new InvalidOperationException(
                $"La cédula {cedula} ya está registrada para este líder");
    }

    private async Task ValidateCedulaNotExistsForLeaderForUpdateAsync(
        int idRegistro, string cedula, int idUser)
    {
        var exists = await _context.Registros
            .AnyAsync(r => r.Cedula == cedula 
                        && r.IdUser == idUser 
                        && r.Id != idRegistro);
        if (exists)
            throw new InvalidOperationException(
                $"La cédula {cedula} ya está registrada para este líder en otro registro");
    }
}