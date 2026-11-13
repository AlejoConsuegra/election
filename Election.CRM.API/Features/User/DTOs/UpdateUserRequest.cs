using System.ComponentModel.DataAnnotations;
using Election.CRM.API.Common.DTOs;

namespace Election.CRM.API.Features.User.DTOs;

public class UpdateUserRequest : UserBaseDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public new string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El email es obligatorio")]
    public new string Email { get; set; } = null!;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    public new string Telefono { get; set; } = null!;

    [Required(ErrorMessage = "El municipio es obligatorio")]
    public new string Municipio { get; set; } = null!;

    [Required(ErrorMessage = "La cédula es obligatoria")]
    public new string Cedula { get; set; } = null!;

    [Required(ErrorMessage = "El rol es obligatorio")]
    public new int IdRol { get; set; }
}