using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.NombreZona.DTOs;

public class UpdateNombreZonaRequest
{
    [Required(ErrorMessage = "El nombre de la zona es obligatorio")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string NameZona { get; set; } = null!;
}