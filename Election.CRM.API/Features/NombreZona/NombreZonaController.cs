using Election.CRM.API.Features.NombreZona.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.RateLimiting;

namespace Election.CRM.API.Features.NombreZona;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class NombreZonaController : ControllerBase
{
    private readonly NombreZonaService _nombreZonaService;

    public NombreZonaController(NombreZonaService nombreZonaService)
    {
        _nombreZonaService = nombreZonaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var nombres = await _nombreZonaService.GetAllNombreZonasAsync();
        return Ok(nombres);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var nombreZona = await _nombreZonaService.GetNombreZonaByIdAsync(id);        
        return Ok(nombreZona);
    }

    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromBody] CreateNombreZonaRequest request)
    {
        var createdNombreZona = await _nombreZonaService.CreateNombreZonaAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = createdNombreZona.IdZona }, createdNombreZona);
    }

    [HttpPut("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateNombreZonaRequest request)
    {
        await _nombreZonaService.UpdateNombreZonaAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Delete(int id)
    {
        await _nombreZonaService.DeleteNombreZonaAsync(id);
        return NoContent();
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchNombreZonaRequest request)
    {
        var results = await _nombreZonaService.SearchByNameAsync(request.NameZona);
        return Ok(results); 
    }
}