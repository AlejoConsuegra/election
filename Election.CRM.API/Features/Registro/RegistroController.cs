using Election.CRM.API.Features.Registro.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

using Election.CRM.API.Common.DTOs;

namespace Election.CRM.API.Features.Registro;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RegistroController : ControllerBase
{
    private readonly RegistroService _registroService;

    public RegistroController(
        RegistroService registroService)
    {
        _registroService = registroService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginatedRequest request, 
        [FromQuery] int? idUser, 
        [FromQuery] string? barrio, 
        [FromQuery] string? cedula)
    {
        if (request.PageSize > 100) 
            request.PageSize = 100;

        var result = await _registroService.GetPaginatedRegistrosAsync(request, idUser, barrio, cedula);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var registro = await _registroService.GetRegistroByIdAsync(id);
        return Ok(registro);
    }

    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromBody] CreateRegistroRequest request)
    {
        var createdRegistro = await _registroService.CreateRegistroAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = createdRegistro.IdRegistro }, createdRegistro);
    }

    [HttpPut("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRegistroRequest request)
    {
        await _registroService.UpdateRegistroAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Delete(int id)
    {
        await _registroService.DeleteRegistroAsync(id);
        return NoContent();
    }
    
    [HttpPost("carga-masiva")]
    [Authorize(Roles = "Admin,Lider")]
    [EnableRateLimiting("write")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> CargaMasiva(IFormFile archivo, [FromQuery] int? idUser)
    {
        var resultado = await _registroService.CargaMasivaAsync(archivo, idUser);
        return Ok(resultado);
    }
}