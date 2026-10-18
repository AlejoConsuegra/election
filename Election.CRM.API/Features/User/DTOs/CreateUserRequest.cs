using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.User.DTOs;

public class CreateUserRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Phone(ErrorMessage = "El teléfono no tiene un formato válido")]
    public string Telefono { get; set; } = null!;

    [Required(ErrorMessage = "El municipio es obligatorio")]
    public string Municipio { get; set; } = null!;

    [Required(ErrorMessage = "La cédula es obligatoria")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "La cédula debe tener entre 5 y 20 caracteres")]
    public string Cedula { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 12, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "El rol es obligatorio")]
    public int IdRol { get; set; }
}