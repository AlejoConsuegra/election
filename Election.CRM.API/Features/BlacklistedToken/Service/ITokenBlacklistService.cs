namespace Election.CRM.API.Features.BlacklistedToken.Services;

public interface ITokenBlacklistService
{
    /// <summary>
    /// Revoca un JWT agregándolo a la blacklist.
    /// </summary>
    Task RevokeAsync(string jti, string userId, DateTime expiresAt, string reason);

    /// <summary>
    /// Verifica si un JWT (identificado por su jti) está revocado.
    /// </summary>
    Task<bool> IsRevokedAsync(string jti);

    /// <summary>
    /// Elimina tokens de la blacklist cuya fecha de expiración ya pasó.
    /// Retorna la cantidad de registros purgados.
    /// </summary>
    Task<int> PurgeExpiredAsync();
}