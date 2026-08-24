using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.AsignacionZona.DTOs;

public class CreateAsignacionZonaRequest
{
    [Required(ErrorMessage = "El ID de la zona es obligatorio")]
    public int IdZona { get; set; }

    [Required(ErrorMessage = "El ID del usuario es obligatorio")]
    public int IdUser { get; set; }
}