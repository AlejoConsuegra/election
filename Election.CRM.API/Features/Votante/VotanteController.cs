using Election.CRM.API.Features.Votante.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

using Election.CRM.API.Common.DTOs;

namespace Election.CRM.API.Features.Votante;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class VotanteController : ControllerBase
{
    private readonly VotanteService _votanteService;

    public VotanteController(VotanteService votanteService)
    {
        _votanteService = votanteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginatedRequest request,
        [FromQuery] int? idUser,
        [FromQuery] string? barrio)
    {
        if (request.PageSize > 100) 
            request.PageSize = 100;

        var result = await _votanteService.GetPaginatedVotantesAsync(request, idUser, barrio);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var votante = await _votanteService.GetVotanteByIdAsync(id);
        return Ok(votante);
    }

    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromBody] CreateVotanteRequest request)
    {
        var createdVotante = await _votanteService.CreateVotanteAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = createdVotante.Id }, createdVotante);
    }

    [HttpPut("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVotanteRequest request)
    {
        await _votanteService.UpdateVotanteAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Delete(int id)
    {
        await _votanteService.DeleteVotanteAsync(id);
        return NoContent();
    }
}