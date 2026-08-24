using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Election.CRM.API.Common.Services;

public class CurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Obtiene el ID del usuario autenticado desde el token JWT
    /// </summary>
    public int GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado");
        }

        if (!int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("El ID del usuario no tiene un formato válido");
        }

        return userId;
    }

    /// <summary>
    /// Obtiene el rol del usuario autenticado desde el token JWT
    /// </summary>
    public string GetUserRole()
    {
        var roleClaim = _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(roleClaim))
        {
            throw new UnauthorizedAccessException("No se pudo identificar el rol del usuario");
        }

        return roleClaim;
    }

    /// <summary>
    /// Valida que el usuario autenticado tenga el rol de Admin
    /// </summary>
    public void ValidateIsAdmin()
    {
        var role = GetUserRole();
        if (role.ToLower() != "admin")
        {
            throw new UnauthorizedAccessException("Solo los administradores pueden realizar esta operación");
        }
    }
}