using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.Votante.DTOs;

public class UpdateVotanteRequest
{
    [Required(ErrorMessage = "La cédula es obligatoria")]
    public string Cedula { get; set; } = null!;
}