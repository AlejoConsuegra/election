using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.User.DTOs;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 3)]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Phone(ErrorMessage = "El formato del celular no es válido")]
    public string Telefono { get; set; } = null!;

    [Required]
    public string Municipio { get; set; } = null!;

    [Required]
    [StringLength(20, MinimumLength = 5)]
    public string Cedula { get; set; } = null!;

    [Required]
    public int IdRol { get; set; }
}