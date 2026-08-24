using Election.CRM.API.Common.Services;
using Election.CRM.API.Data;
using Election.CRM.API.Features.Ocasionale.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Election.CRM.API.Features.Ocasionale.Validators;

public class OcasionaleValidator
{
    private readonly CrmContext _context;
    private readonly CurrentUserService _currentUserService;

    public OcasionaleValidator(CrmContext context, CurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task ValidateCreateAsync(CreateOcasionaleRequest request)
    {
        // 1. Validar permisos (cualquier usuario autenticado puede crear ocasionales)
        var currentUserId = _currentUserService.GetUserId();

        // 2. Validar que la zona exista
        await ValidateZonaExistsAsync(request.IdZona);

        // 3. Validación ESTRICTA: la cédula NO puede estar ya en ocasionales (sin importar el líder)
        await ValidateCedulaNotExistsInOcasionalesAsync(request.Cedula);
    }

    public async Task ValidateUpdateAsync(int idOcasional, UpdateOcasionaleRequest request)
    {
        await ValidateOcasionalExistsAsync(idOcasional);
        await ValidateZonaExistsAsync(request.IdZona);
        await ValidateCedulaNotExistsInOcasionalesForUpdateAsync(idOcasional, request.Cedula);
    }

    public async Task ValidateDeleteAsync(int idOcasional)
    {
        await ValidateOcasionalExistsAsync(idOcasional);
    }

    // ============================================
    // VALIDACIONES PRIVADAS
    // ============================================

    private async Task ValidateOcasionalExistsAsync(int idOcasional)
    {
        var exists = await _context.Ocasionales.AnyAsync(o => o.Id == idOcasional);
        if (!exists)
            throw new KeyNotFoundException($"El ocasional con ID {idOcasional} no existe");
    }

    private async Task ValidateZonaExistsAsync(int idZona)
    {
        var exists = await _context.NombreZonas.AnyAsync(nz => nz.Id == idZona);
        if (!exists)
            throw new InvalidOperationException($"La zona con ID {idZona} no existe");
    }

    /// <summary>
    /// Validación ESTRICTA: la cédula solo puede existir UNA vez en toda la tabla ocasionales,
    /// sin importar qué líder la registró.
    /// </summary>
    private async Task ValidateCedulaNotExistsInOcasionalesAsync(string cedula)
    {
        var existente = await _context.Ocasionales
            .Include(o => o.IdUserNavigation)
            .FirstOrDefaultAsync(o => o.Cedula == cedula);

        if (existente != null)
        {
            throw new InvalidOperationException(
                $"Esta cédula ya fue registrada como ocasional");
        }
    }

    private async Task ValidateCedulaNotExistsInOcasionalesForUpdateAsync(
        int idOcasional, string cedula)
    {
        var existente = await _context.Ocasionales
            .AnyAsync(o => o.Cedula == cedula && o.Id != idOcasional);

        if (existente)
        {
            throw new InvalidOperationException(
                $"Esta cédula ya fue registrada como ocasional por otro líder");
        }
    }
}