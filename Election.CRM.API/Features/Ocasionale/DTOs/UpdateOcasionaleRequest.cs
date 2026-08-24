using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.Ocasionale.DTOs;

public class UpdateOcasionaleRequest
{
    [Required(ErrorMessage = "La cédula es obligatoria")]
    public string Cedula { get; set; } = null!;

    [Required(ErrorMessage = "El nombre del votante es obligatorio")]
    [StringLength(255, MinimumLength = 3)]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "La zona es obligatoria")]
    public int IdZona { get; set; }
}