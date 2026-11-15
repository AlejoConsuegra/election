using System.ComponentModel.DataAnnotations;
using Election.CRM.API.Common.Validation;

namespace Election.CRM.API.Features.Ocasionale.DTOs;

/// <summary>
/// Datos para actualización total de un votante ocasional. Aplica las mismas
/// reglas de formato que la creación. El líder responsable no se modifica desde
/// el cliente: se conserva el asignado o lo reasigna un administrador.
/// </summary>
public class UpdateOcasionaleRequest
{
    /// <summary>
    /// Cédula de ciudadanía del votante. Solo dígitos, sin puntos ni espacios.
    /// </summary>
    /// <example>1045678901</example>
    [Required(ErrorMessage = "La cédula es obligatoria")]
    [CedulaValida]
    public string Cedula { get; set; } = null!;

    /// <summary>
    /// Nombre completo del votante. Solo se admiten letras y espacios.
    /// </summary>
    /// <example>María Fernanda Rodríguez</example>
    [Required(ErrorMessage = "El nombre del votante es obligatorio")]
    [NombreValido]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Identificador de la zona geográfica donde sufragará el votante.
    /// Debe ser un entero positivo y corresponder a una zona existente.
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "La zona es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "El identificador de la zona debe ser un número mayor que cero.")]
    public int IdZona { get; set; }
}