using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Election.CRM.API.Common.Extensions;
using Election.CRM.API.Common.DTOs; 
using Election.CRM.API.Features.User.DTOs;

namespace Election.CRM.API.Features.User;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    // Admin ve Líderes y Coordinadores. Coordinador ve solo Líderes.
    [HttpGet]
    [Authorize(Roles = "Admin, Coordinador")] 
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginatedRequest request,
        [FromQuery] string? nombre,
        [FromQuery] string? cedula,
        [FromQuery] string? email,
        [FromQuery] string? municipio,
        [FromQuery] int? idRol)
    {
        if (request.PageSize > 100) request.PageSize = 100;

        var result = await _userService.GetPaginatedUsersAsync(request, nombre, cedula, email, municipio, idRol);
        return this.Success(result, "usuario creado exitosamente.", 200);
    }

    // Cualquier usuario autenticado puede ver un perfil, pero el Servicio restringirá QUÉ perfiles puede ver.
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);      
        return this.Success(user, "usuario creado exitosamente.", 200);
    }

    // Solo Admin puede crear usuarios.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var createdUser = await _userService.CreateUserAsync(request);
        return this.Success(createdUser, "usuario creado exitosamente.", 200);
    }

    // Solo Admin puede hacer actualización total (Put).
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var updatedUser = await _userService.UpdateUserAsync(id, request);
        return this.Success(updatedUser, "Usuario actualizado exitosamente.", 200);
    }

    // CUALQUIER usuario autenticado puede hacer Patch, PERO el Servicio/Validator bloqueará cambios de rol o de otros usuarios.
    [HttpPatch("{id}")]
    [Authorize] 
    public async Task<IActionResult> Patch(int id, [FromBody] PatchUserRequest request)
    {
        var updatedUser = await _userService.PatchUserAsync(id, request);
        return this.Success(updatedUser, "Usuario actualizado exitosamente.", 200);
    }

    // Solo Admin puede eliminar.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteUserAsync(id);
        return this.Success<object>(null, "Usuario eliminado exitosamente.", 200);
    }
}