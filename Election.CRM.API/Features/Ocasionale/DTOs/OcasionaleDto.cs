namespace Election.CRM.API.Features.Ocasionale.DTOs;

public class OcasionaleDto
{
    public int IdOcasional { get; set; }
    public string Cedula { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public int IdUser { get; set; }
    public string? NombreLider { get; set; } = null!;
    public int IdZona { get; set; }
    public string? NameZona { get; set; } = null!;
    public DateTime FechaOcasional { get; set; }
}