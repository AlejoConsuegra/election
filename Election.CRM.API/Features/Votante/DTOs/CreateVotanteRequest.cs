using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.Votante.DTOs;

public class CreateVotanteRequest
{
    [Required(ErrorMessage = "La cédula es obligatoria")]
    public string Cedula { get; set; } = null!;
}