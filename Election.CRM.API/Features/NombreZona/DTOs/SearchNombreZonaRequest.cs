using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.NombreZona.DTOs;

public class SearchNombreZonaRequest
{
    [Required(ErrorMessage = "El nombre de búsqueda es obligatorio")]
    [MinLength(2, ErrorMessage = "La búsqueda debe tener al menos 2 caracteres")]
    public string NameZona { get; set; } = null!;
}