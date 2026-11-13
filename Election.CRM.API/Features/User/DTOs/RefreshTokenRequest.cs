using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.User.DTOs;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "El refresh token es obligatorio")]
    [StringLength(88, MinimumLength = 88, ErrorMessage = "El refresh token debe tener exactamente 88 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9+/=]+$", ErrorMessage = "El refresh token contiene caracteres inválidos.")]
    public string RefreshToken { get; set; } = null!;
}