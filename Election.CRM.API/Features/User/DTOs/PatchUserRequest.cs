using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.User.DTOs;

public class PatchUserRequest 
{
    [StringLength(100, MinimumLength = 3)]
    public string? Nombre { get; set; }
   
    [EmailAddress]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "El formato del celular no es válido")]
    public string? Telefono { get; set; }

    public string? Municipio { get; set; }

    [StringLength(20, MinimumLength = 5)]
    public string? Cedula { get; set; }

    public int? IdRol { get; set; }
}