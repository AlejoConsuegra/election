using Election.CRM.API.Features.Role.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.RateLimiting;

namespace Election.CRM.API.Features.Role;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class RoleController : ControllerBase
{
    private readonly RoleService _roleService;

    public RoleController(RoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);        
        return Ok(role);
    }

    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var createdRole = await _roleService.CreateRoleAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = createdRole.Id }, createdRole);
    }

    [HttpPut("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleRequest request)
    {
        await _roleService.UpdateRoleAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Delete(int id)
    {
        await _roleService.DeleteRoleAsync(id);
        return NoContent();
    }
}