using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.User.DTOs;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "El refresh token es obligatorio")]
    public string RefreshToken { get; set; } = null!;
}