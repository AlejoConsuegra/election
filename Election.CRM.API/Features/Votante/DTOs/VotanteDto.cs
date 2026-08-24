namespace Election.CRM.API.Features.Votante.DTOs;

public class VotanteDto
{
    public int Id { get; set; }
    public int IdRegistro { get; set; }
    public DateTime FechaVotante { get; set; }
    
    // Datos del votante (vienen de la tabla registros)
    public string? NombreVotante { get; set; } = null!;
    public string? Cedula { get; set; }
    public string? Direccion { get; set; }
    public string? Barrio { get; set; }
    public string? Celular { get; set; }
    public string? Puesto { get; set; }
    
    // Datos del líder
    public string? NombreLider { get; set; } = null!;
    
    // Datos de la zona
    public string? NameZona { get; set; } = null!;
}