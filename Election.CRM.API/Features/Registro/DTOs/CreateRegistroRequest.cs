using System.ComponentModel.DataAnnotations;
using Election.CRM.API.Common.Validation;

namespace Election.CRM.API.Features.Registro.DTOs;

/// <summary>
/// Datos para registrar un votante. Todos los campos salvo IdUser son obligatorios.
/// Las reglas de formato provienen de la biblioteca común de validación positiva.
/// </summary>
public class CreateRegistroRequest
{
    /// <summary>
    /// Líder al que se asigna el votante. El Admin lo envía para seleccionar líder;
    /// el Líder lo omite y el sistema toma el identificador de su sesión.
    /// </summary>
    /// <example>2</example>
    [Range(1, int.MaxValue, ErrorMessage = "El identificador del líder debe ser un número mayor que cero.")]
    public int? IdUser { get; set; }

    /// <summary>
    /// Nombre completo del votante. Solo se admiten letras y espacios.
    /// </summary>
    /// <example>María Fernanda Rodríguez</example>
    [Required(ErrorMessage = "El nombre del votante es obligatorio")]
    [NombreValido]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Cédula de ciudadanía del votante. Solo dígitos, sin puntos ni espacios.
    /// </summary>
    /// <example>1045678901</example>
    [Required(ErrorMessage = "La cédula es obligatoria")]
    [CedulaValida]
    public string Cedula { get; set; } = null!;

    /// <summary>
    /// Dirección de residencia del votante.
    /// </summary>
    /// <example>Calle 45 # 12-34</example>
    [Required(ErrorMessage = "La dirección es obligatoria")]
    [DireccionValida]
    public string Direccion { get; set; } = null!;

    /// <summary>
    /// Barrio, vereda o urbanización de residencia.
    /// </summary>
    /// <example>El Carmen</example>
    [Required(ErrorMessage = "El barrio es obligatorio")]
    [BarrioValido]
    public string Barrio { get; set; } = null!;

    /// <summary>
    /// Teléfono de contacto. Fijo local de 7 dígitos o móvil/nacional de 10 dígitos.
    /// </summary>
    /// <example>3001234567</example>
    [Required(ErrorMessage = "El celular es obligatorio")]
    [TelefonoValido]
    public string Celular { get; set; } = null!;

    /// <summary>
    /// Puesto de votación asignado al votante.
    /// </summary>
    /// <example>IE Distrital La Esperanza</example>
    [Required(ErrorMessage = "El puesto es obligatorio")]
    [PuestoValido]
    public string Puesto { get; set; } = null!;
}