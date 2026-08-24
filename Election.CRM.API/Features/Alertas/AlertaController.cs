using Election.CRM.API.Features.Alertas.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Election.CRM.API.Features.Alertas;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AlertaController : ControllerBase
{
    private readonly AlertaService _alertaService;

    public AlertaController(AlertaService alertaService)
    {
        _alertaService = alertaService;
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen()
    {
        var resumen = await _alertaService.GetResumenAsync();
        return Ok(resumen);
    }

    [HttpGet("registros")]
    public async Task<IActionResult> GetAlertasRegistros()
    {
        var alertas = await _alertaService.GetAlertasRegistrosAsync();
        return Ok(alertas);
    }

    [HttpGet("votantes")]
    public async Task<IActionResult> GetAlertasVotantes()
    {
        var alertas = await _alertaService.GetAlertasVotantesAsync();
        return Ok(alertas);
    }
}