using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.Role.DTOs;

public class CreateRoleRequest
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
    public string NameRol { get; set; } = null!;
}