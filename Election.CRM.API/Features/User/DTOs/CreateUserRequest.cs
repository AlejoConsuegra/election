using System.ComponentModel.DataAnnotations;
using Election.CRM.API.Common.Validation;

namespace Election.CRM.API.Features.User.DTOs;

/// <summary>
/// Datos para crear un usuario. Todos los campos son obligatorios.
/// Las reglas de formato provienen de la biblioteca común de validación positiva.
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Nombre completo de la persona. Solo se admiten letras y espacios.
    /// </summary>
    /// <example>Juan Pérez Gómez</example>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [NombreValido]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Cédula de ciudadanía. Solo dígitos, sin puntos ni espacios.
    /// </summary>
    /// <example>1000123456</example>
    [Required(ErrorMessage = "La cédula es obligatoria")]
    [CedulaValida]
    public string Cedula { get; set; } = null!;

    /// <summary>
    /// Correo electrónico de contacto con formato válido.
    /// </summary>
    /// <example>juan.perez@campana.com</example>
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailValido]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Teléfono de contacto. Fijo local de 7 dígitos o móvil/nacional de 10 dígitos.
    /// </summary>
    /// <example>3001234567</example>
    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [TelefonoValido]
    public string Telefono { get; set; } = null!;

    /// <summary>
    /// Municipio de residencia. Solo se admiten letras y espacios.
    /// </summary>
    /// <example>Barranquilla</example>
    [Required(ErrorMessage = "El municipio es obligatorio")]
    [MunicipioValido]
    public string Municipio { get; set; } = null!;

    /// <summary>
    /// Contraseña en texto plano. Mínimo 12 caracteres. Se almacena con hash BCrypt.
    /// </summary>
    /// <example>Campana2026Segura</example>
    [Required(AllowEmptyStrings = false, ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(12, ErrorMessage = "La contraseña debe tener al menos 12 caracteres.")]
    [MaxLength(100, ErrorMessage = "La contraseña no puede exceder 100 caracteres.")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// Rol asignado. Solo se admiten Líder (2) y Coordinador (3).
    /// El rol Admin (1) no puede crearse desde la API.
    /// </summary>
    /// <example>2</example>
    [Required(ErrorMessage = "El rol es obligatorio")]
    [RolAsignable]
    public int IdRol { get; set; }
}