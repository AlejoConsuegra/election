using Election.CRM.API.Common.Services;
using Election.CRM.API.Data;
using Election.CRM.API.Features.Votante.DTOs;
using Microsoft.EntityFrameworkCore;

using ExceptionCedula = Election.CRM.API.Common.Exceptions.CedulaNoRegistradaException;

namespace Election.CRM.API.Features.Votante.Validators;

public class VotanteValidator
{
    private readonly CrmContext _context;
    private readonly CurrentUserService _currentUserService;

    public VotanteValidator(CrmContext context, CurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /* public async Task ValidateCreateAsync(CreateVotanteRequest request)
    {
        // 1. Obtener el IdUser del líder autenticado
        var currentUserId = _currentUserService.GetUserId();

        // 2. Validar que el líder exista
        await ValidateUserExistsAsync(currentUserId);

        // 3. Validar que la cédula exista en registros
        await ValidateRegistroExistsAsync(request.Cedula);

        // 4. Validar que NO exista la misma cédula para el mismo líder (bloquear)
        await ValidateCedulaNotAlreadyVotedByLeaderAsync(request.Cedula, currentUserId);
    } */

    public async Task ValidateCreateAsync(int registroId, int currentUserId)
    {
        // Validar que NO exista ya un voto para este registro específico.
        // Como la relación es 1:1 y tenemos un índice único en IdRegistro, esta es la forma más eficiente.
        var exists = await _context.Votantes
            .AnyAsync(v => v.IdRegistro == registroId);

        if (exists)
        {
            throw new InvalidOperationException("Este votante ya fue registrado con su cédula por usted anteriormente.");
        }
        
        // Nota: Las validaciones de "User exists" y "Registro exists" ya las cubrió 
        // el Service al hacer el FirstOrDefaultAsync, por lo que no es necesario repetirlas aquí.
    }

    public async Task ValidateUpdateAsync(int idVotante, UpdateVotanteRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();

        await ValidateVotanteExistsAsync(idVotante);
        await ValidateRegistroExistsAsync(request.Cedula);
        await ValidateCedulaNotAlreadyVotedByLeaderForUpdateAsync(idVotante, request.Cedula, currentUserId);
    }

    public async Task ValidateDeleteAsync(int idVotante)
    {
        await ValidateVotanteExistsAsync(idVotante);
    }

    // ============================================
    // VALIDACIONES PRIVADAS
    // ============================================

    private async Task ValidateVotanteExistsAsync(int idVotante)
    {
        var exists = await _context.Votantes.AnyAsync(v => v.Id == idVotante);
        if (!exists)
            throw new KeyNotFoundException($"El votante con ID {idVotante} no existe");
    }

    // Nota: Esto debería ir en la entidad User
    private async Task ValidateUserExistsAsync(int idUser)
    {
        var exists = await _context.Users.AnyAsync(u => u.Id == idUser);
        if (!exists)
            throw new InvalidOperationException($"El líder con ID {idUser} no existe");
    }

    // Nota: Esto debería ir en la entidad Registro
    private async Task ValidateRegistroExistsAsync(string cedula)
    {
        var exists = await _context.Registros.AnyAsync(r => r.Cedula == cedula);
        if (!exists)
            throw new ExceptionCedula(cedula);
    }

    private async Task ValidateCedulaNotAlreadyVotedByLeaderAsync(string cedula, int idUser)
    {
        var exists = await _context.Votantes
            .AnyAsync(v => v.IdRegistroNavigation.Cedula == cedula
                        && v.IdRegistroNavigation.IdUser == idUser);
        if (exists)
            throw new InvalidOperationException("Este votante ya fue registrado por usted");
    }

    private async Task ValidateCedulaNotAlreadyVotedByLeaderForUpdateAsync(
        int idVotante, string cedula, int idUser)
    {
        // EF Core traducirá esto a un EXISTS con JOIN, sin cargar datos innecesarios
        var exists = await _context.Votantes
            .AnyAsync(v => v.IdRegistroNavigation.Cedula == cedula 
                        && v.IdRegistroNavigation.IdUser == idUser 
                        && v.Id != idVotante);

        if (exists)
        {
            throw new InvalidOperationException("Esta cédula ya fue registrada como votante por usted anteriormente.");
        }
    }
}