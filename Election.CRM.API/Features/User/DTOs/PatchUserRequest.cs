using Election.CRM.API.Common.Validation;

namespace Election.CRM.API.Features.User.DTOs;

/// <summary>
/// Datos para actualización parcial de un usuario. Todos los campos son opcionales:
/// solo se modifican los que se envíen. Las reglas de formato se aplican únicamente
/// a los campos presentes; los ausentes se omiten sin generar rechazo.
/// </summary>
public class PatchUserRequest
{
    /// <summary>
    /// Nombre completo de la persona. Solo se admiten letras y espacios.
    /// </summary>
    /// <example>Juan Pérez Gómez</example>
    [NombreValido]
    public string? Nombre { get; set; }

    /// <summary>
    /// Cédula de ciudadanía. Solo dígitos, sin puntos ni espacios.
    /// </summary>
    /// <example>1000123456</example>
    [CedulaValida]
    public string? Cedula { get; set; }

    /// <summary>
    /// Correo electrónico de contacto con formato válido.
    /// </summary>
    /// <example>juan.perez@campana.com</example>
    [EmailValido]
    public string? Email { get; set; }

    /// <summary>
    /// Teléfono de contacto. Fijo local de 7 dígitos o móvil/nacional de 10 dígitos.
    /// </summary>
    /// <example>3001234567</example>
    [TelefonoValido]
    public string? Telefono { get; set; }

    /// <summary>
    /// Municipio de residencia. Solo se admiten letras y espacios.
    /// </summary>
    /// <example>Barranquilla</example>
    [MunicipioValido]
    public string? Municipio { get; set; }

    /// <summary>
    /// Rol asignado. Solo se admiten Líder (2) y Coordinador (3).
    /// Su modificación está restringida al rol Admin en la capa de servicio.
    /// </summary>
    /// <example>2</example>
    [RolAsignable]
    public int? IdRol { get; set; }
}