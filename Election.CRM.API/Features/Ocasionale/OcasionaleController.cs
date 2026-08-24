using Election.CRM.API.Features.Ocasionale.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

using Election.CRM.API.Common.DTOs;

namespace Election.CRM.API.Features.Ocasionale;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OcasionaleController : ControllerBase
{
    private readonly OcasionaleService _ocasionaleService;

    public OcasionaleController(OcasionaleService ocasionaleService)
    {
        _ocasionaleService = ocasionaleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginatedRequest request,
        [FromQuery] int? idUser,
        [FromQuery] int? idZona,
        [FromQuery] string? cedula)
    {
        if (request.PageSize > 100) 
            request.PageSize = 100;

        var result = await _ocasionaleService.GetPaginatedOcasionalesAsync(request, idUser, idZona, cedula);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ocasional = await _ocasionaleService.GetOcasionalByIdAsync(id);
        return Ok(ocasional);
    }

    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromBody] CreateOcasionaleRequest request)
    {
        var createdOcasional = await _ocasionaleService.CreateOcasionalAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = createdOcasional.IdOcasional }, createdOcasional);
    }

    [HttpPut("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOcasionaleRequest request)
    {
        await _ocasionaleService.UpdateOcasionalAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Delete(int id)
    {
        await _ocasionaleService.DeleteOcasionalAsync(id);
        return NoContent();
    }
}