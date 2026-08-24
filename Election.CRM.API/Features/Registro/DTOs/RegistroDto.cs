namespace Election.CRM.API.Features.Registro.DTOs;

public class RegistroDto
{
    public int IdRegistro { get; set; }
    public int? IdUser { get; set; }
    public string? NombreLider { get; set; }  // Viene de users.nombre por JOIN
    public string Nombre { get; set; } = null!;  // Nombre del votante
    public string Cedula { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string Barrio { get; set; } = null!;
    public string Celular { get; set; } = null!;
    public string Puesto { get; set; } = null!;
}