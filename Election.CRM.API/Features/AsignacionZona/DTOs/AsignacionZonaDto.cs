namespace Election.CRM.API.Features.AsignacionZona.DTOs;

public class AsignacionZonaDto
{
    public int IdAsig { get; set; }
    public int IdZona { get; set; }
    public string? NameZona { get; set; } = null!;
    public int IdUser { get; set; }
    public string? NombreUsuario { get; set; } = null!;
    public DateTime FechaAsignacion { get; set; }
}