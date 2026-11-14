using Microsoft.EntityFrameworkCore;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly JwtService _jwtService;
    private readonly LoginAttemptService _loginAttemptService;
    private readonly RefreshTokenService _refreshTokenService;

    public UserService(
        CrmContext context, 
        UserValidator validator, 
        ICurrentUserService currentUserService, // <-- Inyección limpia
        JwtService jwtService,
        LoginAttemptService loginAttemptService,
        RefreshTokenService refreshTokenService)
    {
        _context = context;
        _validator = validator;
        _currentUserService = currentUserService;
        _jwtService = jwtService;
        _loginAttemptService = loginAttemptService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<PaginatedResponse<UserDto>> GetPaginatedUsersAsync(
        PaginatedRequest request,
        string? nombre, string? cedula, string? email, string? municipio, int? idRol)
    {
        var query = _context.Users.Include(u => u.IdRolNavigation).AsQueryable();

        // 1. FILTRADO JERÁRQUICO (Código limpio usando el servicio centralizado)
        if (_currentUserService.RoleName == "Coordinator")
        {
            // Coordinador solo puede ver Líderes (IdRol = 2)
            query = query.Where(u => u.IdRol == 2);
        }
        else if (_currentUserService.RoleName == "Lider")
        {
            // Líder solo puede verse a sí mismo
            query = query.Where(u => u.Id == _currentUserService.UserId);
        }
        else // Admin u otros roles con privilegios
        {
            // Admin ve todos, pero ocultamos al SuperAdmin (Id=1) por seguridad
            query = query.Where(u => u.IdRol != 1); 
        }

        // 2. Filtros específicos (se aplican sobre el conjunto ya filtrado por seguridad)
        if (!string.IsNullOrWhiteSpace(nombre)) query = query.Where(u => u.Nombre.Contains(nombre));
        if (!string.IsNullOrWhiteSpace(cedula)) query = query.Where(u => u.Cedula.Contains(cedula));
        if (!string.IsNullOrWhiteSpace(email)) query = query.Where(u => u.Email.Contains(email));
        if (!string.IsNullOrWhiteSpace(municipio)) query = query.Where(u => u.Municipio.Contains(municipio));
        if (idRol.HasValue) query = query.Where(u => u.IdRol == idRol.Value);

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
        // VALIDACIÓN DE MÍNIMO PRIVILEGIO PARA LECTURA
        if (_currentUserService.RoleName == "Lider" && _currentUserService.UserId != id)
        {
            throw new UnauthorizedAccessException("No tienes permiso para ver el perfil de otro usuario.");
        }

        if (_currentUserService.RoleName == "Coordinator")
        {
            var targetUser = await _context.Users.FindAsync(id);
            if (targetUser != null && targetUser.IdRol != 2) // 2 = Líder
            {
                throw new UnauthorizedAccessException("Como Coordinador, solo puedes ver perfiles de Líderes.");
            }
        }

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

        // DEFENSA EN PROFUNDIDAD: Validación explícita de la contraseña
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 12)
        {
            throw new InvalidOperationException("La contraseña es obligatoria y debe tener al menos 12 caracteres.");
        }
        
        // DEFENSA EN PROFUNDIDAD: Bloquear explícitamente la creación de Admins desde la API
        if (request.IdRol == 1)
        {
            throw new InvalidOperationException("No se permite la creación de usuarios con rol Admin a través de esta API.");
        }

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

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        await _validator.ValidateUpdateAsync(id, request);

        // DEFENSA EN PROFUNDIDAD: Bloquear asignación de rol Admin en actualización total
        if (request.IdRol == 1)
        {
            throw new InvalidOperationException("No se permite asignar el rol Admin a través de esta API.");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null) throw new KeyNotFoundException($"Usuario con ID {id} no encontrado");

        user.Nombre = request.Nombre;
        user.Email = request.Email;
        user.Telefono = request.Telefono;
        user.Municipio = request.Municipio;
        user.Cedula = request.Cedula;
        user.IdRol = request.IdRol;

        await _context.SaveChangesAsync();
        
        return (await GetUserByIdAsync(user.Id))!;
    }

    public async Task<UserDto> PatchUserAsync(int id, PatchUserRequest request)
    {
        // 1. Mínimo Privilegio: Un usuario no-admin solo puede modificar su propio registro
        if (!_currentUserService.IsAdmin && _currentUserService.UserId != id)
        {
            throw new UnauthorizedAccessException("No tienes permiso para modificar los datos de otro usuario.");
        }

        // 2. Integridad: Si se intenta cambiar el rol, debe ser Admin y NUNCA puede ser rol 1 (Admin)
        if (request.IdRol.HasValue)
        {
            if (!_currentUserService.IsAdmin)
            {
                throw new UnauthorizedAccessException("Solo un Administrador puede modificar el rol de un usuario.");
            }
            if (request.IdRol.Value == 1)
            {
                throw new InvalidOperationException("El rol Admin no puede ser asignado a través de esta operación.");
            }
        }

        // 3. Aplicar cambios parciales
        var user = await _context.Users.FindAsync(id);
        if (user == null) throw new KeyNotFoundException("Usuario no encontrado");

        if(request.Nombre != null) user.Nombre = request.Nombre;
        if(request.Email != null) user.Email = request.Email;
        if(request.Telefono != null) user.Telefono = request.Telefono;
        if(request.Municipio != null) user.Municipio = request.Municipio;
        if(request.Cedula != null) user.Cedula = request.Cedula;
        if(request.IdRol.HasValue) user.IdRol = request.IdRol.Value;

        await _context.SaveChangesAsync();

        return (await GetUserByIdAsync(user.Id))!;
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