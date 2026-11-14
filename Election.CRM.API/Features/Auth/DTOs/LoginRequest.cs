using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.Auth.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "La cédula es obligatoria")]
    // Validación Positiva: Solo permite números y longitud entre 6 y 12 dígitos (estándar cédulas).
    // ^[0-9]$: Solo dígitos. {6,12}: Longitud mínima 6, máxima 12.
    [RegularExpression(@"^[0-9]{6,12}$", ErrorMessage = "La cédula debe contener solo números y tener entre 6 y 12 dígitos.")]
    public string Cedula { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(12, ErrorMessage = "La contraseña debe tener al menos 12 caracteres.")] // Mensaje corregido
    public string Password { get; set; } = null!;
}