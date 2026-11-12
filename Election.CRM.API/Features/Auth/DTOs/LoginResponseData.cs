namespace Election.CRM.API.Features.Auth.DTOs;

public class LoginResponseData
{
    public string Nombre { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Rol { get; set; } = null!;
}
