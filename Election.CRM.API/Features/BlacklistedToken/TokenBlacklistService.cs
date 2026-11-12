using Microsoft.EntityFrameworkCore;
using Election.CRM.API.Data;

using Election.CRM.API.Features.BlacklistedToken;

namespace Election.CRM.API.Features.BlacklistedToken.Services;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly CrmContext _context;

    public TokenBlacklistService(CrmContext context)
    {
        _context = context;
    }

    public async Task RevokeAsync(string jti, string userId, DateTime expiresAt, string reason)
    {
        if (string.IsNullOrWhiteSpace(jti))
            throw new ArgumentException("El JTI es obligatorio para revocar un token.", nameof(jti));

        // Idempotente: si ya existe, no duplicamos
        var alreadyRevoked = await _context.BlacklistedTokens
            .AnyAsync(b => b.Jti == jti);

        if (alreadyRevoked) return;

        var entry = new BlacklistedToken
        {
            Id = Guid.NewGuid(),
            Jti = jti,
            UserId = userId,
            ExpiresAt = expiresAt.ToUniversalTime(),
            RevokedAt = DateTime.UtcNow,
            Reason = reason
        };

        _context.BlacklistedTokens.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsRevokedAsync(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti)) return false;

        return await _context.BlacklistedTokens
            .AsNoTracking()
            .AnyAsync(b => b.Jti == jti);
    }

    public async Task<int> PurgeExpiredAsync()
    {
        var now = DateTime.UtcNow;
        var expired = await _context.BlacklistedTokens
            .Where(b => b.ExpiresAt < now)
            .ToListAsync();

        if (expired.Count == 0) return 0;

        _context.BlacklistedTokens.RemoveRange(expired);
        return await _context.SaveChangesAsync();
    }
}