// Features/Auth/Entities/BlacklistedToken.cs
namespace Election.CRM.API.Features.BlacklistedToken;

public class BlacklistedToken
{
    public Guid Id { get; set; }
    public string Jti { get; set; } = string.Empty;       // Identificador único del JWT (claim "jti")
    public string UserId { get; set; } = string.Empty;     // Dueño del token
    public DateTime ExpiresAt { get; set; }                // Para purgar tokens ya expirados
    public DateTime RevokedAt { get; set; }                // Cuándo se revocó
    public string? Reason { get; set; }                    // "logout", "password_change", etc.
}