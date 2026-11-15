using System.ComponentModel.DataAnnotations;
using Election.CRM.API.Common.Validation;

namespace Election.CRM.API.Features.Ocasionale.DTOs;

/// <summary>
/// Datos para registrar un votante ocasional, es decir, un votante que sufragará
/// en una zona distinta a la de su residencia. El líder responsable se toma de la
/// sesión autenticada y no se acepta desde el cliente.
/// </summary>
public class CreateOcasionaleRequest
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