using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Election.CRM.API.Common.Services;

public class CurrentUserService : ICurrentUserService
{
    // 1. Campos privados para almacenar los datos del JWT (se leen UNA sola vez por petición)
    private readonly int _userId;
    private readonly string _roleName;
    private readonly bool _isAdmin;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;

        // Si no hay usuario autenticado (ej. endpoint con [AllowAnonymous]), dejamos valores por defecto
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            _userId = 0;
            _roleName = string.Empty;
            _isAdmin = false;
            return;
        }

        // 2. LEEMOS EL JWT UNA SOLA VEZ AQUÍ (Equivalente a tu setUser de PHP)
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _roleName = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        _userId = int.TryParse(userIdClaim, out int id) ? id : 0;
        _isAdmin = _roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }

    // ==========================================
    // 3. PROPIEDADES NUEVAS (Para código más limpio en el futuro)
    // ==========================================
    public int UserId => _userId;
    public string RoleName => _roleName;
    public bool IsAdmin => _isAdmin;

    // ==========================================
    // 4. MÉTODOS EXISTENTES (Intactos para no romper nada)
    // ==========================================

    /// <summary>
    /// Obtiene el ID del usuario autenticado desde el token JWT
    /// </summary>
    public int GetUserId()
    {
        if (_userId == 0)
        {
            throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado");
        }
        return _userId;
    }

    /// <summary>
    /// Obtiene el rol del usuario autenticado desde el token JWT
    /// </summary>
    public string GetUserRole()
    {
        if (string.IsNullOrEmpty(_roleName))
        {
            throw new UnauthorizedAccessException("No se pudo identificar el rol del usuario");
        }
        return _roleName;
    }

    /// <summary>
    /// Valida que el usuario autenticado tenga el rol de Admin
    /// </summary>
    public void ValidateIsAdmin()
    {
        if (!_isAdmin)
        {
            throw new UnauthorizedAccessException("Solo los administradores pueden realizar esta operación");
        }
    }
}