using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.Auth.DTOs;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "El refresh token es obligatorio")]
    // Validación Positiva: Longitud exacta de un Base64 de 64 bytes.
    [StringLength(88, MinimumLength = 88, ErrorMessage = "El refresh token debe tener exactamente 88 caracteres.")]
    // Validación Positiva: Solo permite caracteres válidos de Base64.
    [RegularExpression(@"^[A-Za-z0-9+/=]+$", ErrorMessage = "El refresh token contiene caracteres inválidos.")]
    public string RefreshToken { get; set; } = null!;
}