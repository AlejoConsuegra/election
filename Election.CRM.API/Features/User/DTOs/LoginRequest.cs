/* using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.User.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "La cédula es obligatoria")]
    public string Cedula { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(12, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string Password { get; set; } = null!;
} */