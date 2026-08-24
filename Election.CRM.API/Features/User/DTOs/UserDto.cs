namespace Election.CRM.API.Features.User.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public string Municipio { get; set; } = null!;
    public string Cedula { get; set; } = null!;
    public string? NombreRol { get; set; } = null!; 
}