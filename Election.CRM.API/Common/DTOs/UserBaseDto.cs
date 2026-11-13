using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Common.DTOs;

/// <summary>
/// Clase base que centraliza las reglas de Validación Positiva para datos de usuario.
/// Reutilizable en cualquier DTO que maneje creación o edición de usuarios.
/// </summary>
public abstract class UserBaseDto
{
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    public string? Nombre { get; set; }

    [RegularExpression(@"^[0-9]{6,12}$", ErrorMessage = "La cédula debe contener solo números y tener entre 6 y 12 dígitos.")]
    public string? Cedula { get; set; }

    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres.")]
    public string? Email { get; set; }

    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "El teléfono debe tener exactamente 10 dígitos numéricos.")]
    public string? Telefono { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "El municipio debe tener entre 2 y 100 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El municipio solo puede contener letras y espacios.")]
    public string? Municipio { get; set; }
    
    [Range(2, 3, ErrorMessage = "El rol debe ser Líder (2) o Coordinador (3). El rol Admin no se puede asignar desde esta operación.")]
    public int? IdRol { get; set; }
}