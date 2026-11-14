using Microsoft.EntityFrameworkCore;
using Election.CRM.API.Data;
using Election.CRM.API.Features.Votante.DTOs;

using ExceptionCedula = Election.CRM.API.Common.Exceptions.CedulaNoRegistradaException;

namespace Election.CRM.API.Features.Votante.Validators;

public class VotanteValidator
{
    private readonly CrmContext _context;

    public VotanteValidator(CrmContext context)
    {
        _context = context;
    }

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
    }
}