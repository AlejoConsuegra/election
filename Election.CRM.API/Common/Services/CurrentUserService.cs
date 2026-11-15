using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Election.CRM.API.Common.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly int _userId;
    private readonly string _roleName;
    private readonly string _nombre;
    private readonly string _email;
    private readonly bool _isAdmin;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            _userId = 0;
            _roleName = string.Empty;
            _nombre = string.Empty;
            _email = string.Empty;
            _isAdmin = false;
            return;
        }

        // Leemos los claims UNA sola vez
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _roleName = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        _nombre = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        _email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? string.Empty;

        _userId = int.TryParse(userIdClaim, out int id) ? id : 0;
        _isAdmin = _roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }

    public int UserId => _userId;
    public string RoleName => _roleName;
    public string Nombre => _nombre;
    public string Email => _email;
    public bool IsAdmin => _isAdmin;

    public int GetUserId()
    {
        if (_userId == 0) throw new UnauthorizedAccessException("No se pudo identificar al usuario autenticado");
        return _userId;
    }

    public string GetUserRole()
    {
        if (string.IsNullOrEmpty(_roleName)) throw new UnauthorizedAccessException("No se pudo identificar el rol del usuario");
        return _roleName;
    }

    public void ValidateIsAdmin()
    {
        if (!_isAdmin) throw new UnauthorizedAccessException("Solo los administradores pueden realizar esta operación");
    }
}