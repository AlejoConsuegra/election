using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
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
        return Ok(result);
    }

    // Cualquier usuario autenticado puede ver un perfil, pero el Servicio restringirá QUÉ perfiles puede ver.
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);        
        return Ok(user);
    }

    // Solo Admin puede crear usuarios.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var createdUser = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
    }

    // Solo Admin puede hacer actualización total (Put).
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        await _userService.UpdateUserAsync(id, request);
        return NoContent();
    }

    // CUALQUIER usuario autenticado puede hacer Patch, PERO el Servicio/Validator bloqueará cambios de rol o de otros usuarios.
    [HttpPatch("{id}")]
    [Authorize] 
    public async Task<IActionResult> Patch(int id, [FromBody] PatchUserRequest request)
    {
        await _userService.PatchUserAsync(id, request);
        return NoContent();
    }

    // Solo Admin puede eliminar.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Delete(int id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }
}