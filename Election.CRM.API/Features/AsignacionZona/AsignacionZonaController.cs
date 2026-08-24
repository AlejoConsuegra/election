using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

using Election.CRM.API.Features.AsignacionZona.DTOs;
using Election.CRM.API.Common.DTOs; 

namespace Election.CRM.API.Features.AsignacionZona;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AsignacionZonaController : ControllerBase
{
    private readonly AsignacionZonaService _asignacionZonaService;

    public AsignacionZonaController(AsignacionZonaService asignacionZonaService)
    {
        _asignacionZonaService = asignacionZonaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginatedRequest request,
        [FromQuery] int? idUser,
        [FromQuery] int? idZona)
    {
        if (request.PageSize > 100) 
            request.PageSize = 100;

        var result = await _asignacionZonaService.GetPaginatedAsignacionesAsync(request, idUser, idZona);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var asignacionZona = await _asignacionZonaService.GetAsignacionZonaByIdAsync(id);        
        return Ok(asignacionZona);
    }

    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromBody] CreateAsignacionZonaRequest request)
    {
        var createdZOna = await _asignacionZonaService.CreateAsignacionZonaAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = createdZOna.IdZona }, createdZOna);
    }

    [HttpPut("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAsignacionZonaRequest request)
    {
        await _asignacionZonaService.UpdateAsignacionZonaAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Delete(int id)
    {
        await _asignacionZonaService.DeleteAsignacionZonaAsync(id);
        return NoContent();
    }
}