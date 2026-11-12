using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Election.CRM.API.Common.Services;
using Election.CRM.API.Common.Helpers;
using Election.CRM.API.Common.DTOs; 
using Election.CRM.API.Data;

using Election.CRM.API.Features.User.Validators;
using Election.CRM.API.Features.User.DTOs;

namespace Election.CRM.API.Features.User;

public class UserService
{
    private readonly CrmContext _context;
    private readonly UserValidator _validator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtService _jwtService;
    private readonly LoginAttemptService _loginAttemptService;
    private readonly RefreshTokenService _refreshTokenService;

    public UserService(
        CrmContext context, 
        UserValidator validator, 
        IHttpContextAccessor httpContextAccessor, 
        JwtService jwtService,
        LoginAttemptService loginAttemptService,
        RefreshTokenService refreshTokenService)
    {
        _context = context;
        _validator = validator;
        _httpContextAccessor = httpContextAccessor;
        _jwtService = jwtService;
        _loginAttemptService = loginAttemptService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.IdRolNavigation)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Telefono = u.Telefono,
                Municipio = u.Municipio,
                Cedula = u.Cedula,
                NombreRol = u.IdRolNavigation.NameRol
            })
            .ToListAsync();
    }

    public async Task<PaginatedResponse<UserDto>> GetPaginatedUsersAsync(
    PaginatedRequest request,
    string? nombre,
    string? cedula,
    string? email,
    string? municipio,
    int? idRol)
    {
        var query = _context.Users
            .Include(u => u.IdRolNavigation)
            .AsQueryable();

        query = query.Where(u => u.IdRol != 1);

        // Filtros específicos
        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(u => u.Nombre.Contains(nombre));
        if (!string.IsNullOrWhiteSpace(cedula))
            query = query.Where(u => u.Cedula.Contains(cedula));
        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(u => u.Email.Contains(email));
        if (!string.IsNullOrWhiteSpace(municipio))
            query = query.Where(u => u.Municipio.Contains(municipio));
        if (idRol.HasValue)
            query = query.Where(u => u.IdRol == idRol.Value);

        // Búsqueda general
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u => u.Nombre.Contains(request.SearchTerm) ||
                                    u.Cedula.Contains(request.SearchTerm) ||
                                    u.Email.Contains(request.SearchTerm));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Telefono = u.Telefono,
                Municipio = u.Municipio,
                Cedula = u.Cedula,
                NombreRol = u.IdRolNavigation != null ? u.IdRolNavigation.NameRol : null
            })
            .ToListAsync();

        return new PaginatedResponse<UserDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.IdRolNavigation)
            .Where(u => u.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Email = u.Email,
                Telefono = u.Telefono,
                Municipio = u.Municipio,
                Cedula = u.Cedula,
                NombreRol = u.IdRolNavigation.NameRol
            })
            .FirstOrDefaultAsync();

        if (user == null)
            throw new KeyNotFoundException($"Usuario con ID {id} no encontrado");
        
        return user;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        await _validator.ValidateCreateAsync(request);
        
        var hashedPassword = PasswordHelper.HashPassword(request.Password);
        
        var user = new User
        {
            Nombre = request.Nombre,
            IdRol = request.IdRol,
            Email = request.Email,
            Telefono = request.Telefono,
            Municipio = request.Municipio,
            Cedula = request.Cedula,
            Password = hashedPassword
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return (await GetUserByIdAsync(user.Id))!;
    }

    public async Task UpdateUserAsync(int id, UpdateUserRequest request)
    {
        await _validator.ValidateUpdateAsync(id, request);

        var user = await _context.Users.FindAsync(id);
        if (user == null) return;

        user.Nombre = request.Nombre;
        user.Email = request.Email;
        user.Telefono = request.Telefono;
        user.Municipio = request.Municipio;
        user.Cedula = request.Cedula;
        user.IdRol = request.IdRol;

        await _context.SaveChangesAsync();
    }

    public async Task PatchUserAsync(int id, PatchUserRequest request)
    {
        // VALIDACIÓN DE MÍNIMO PRIVILEGIO (C-02)
        // Solo exigimos ser Admin si el request intenta modificar el IdRol.
        // Esto permite que un usuario pueda actualizar sus propios datos (teléfono, email) 
        // sin necesitar permisos de administrador, pero bloquea la escalada de privilegios.
        if (request.IdRol.HasValue)
        {
            await _validator.ValidateIsAdminAsync();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null) return;

        if(request.Nombre != null)
            user.Nombre = request.Nombre;

        if(request.Email != null)
            user.Email = request.Email;

        if(request.Telefono != null)
            user.Telefono = request.Telefono;

        if(request.Municipio != null)
            user.Municipio = request.Municipio;

        if(request.Cedula != null)
            user.Cedula = request.Cedula;

        if(request.IdRol.HasValue)
            user.IdRol = request.IdRol.Value;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        await _validator.ValidateDeleteAsync(id);

        var user = await _context.Users.FindAsync(id);
        if(user == null) return;
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}