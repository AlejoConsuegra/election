using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Election.CRM.API.Common.Services;

public class RefreshTokenService
{
    private readonly ConcurrentDictionary<string, RefreshTokenInfo> _tokens = new();
    private readonly TimeSpan _refreshTokenExpiration = TimeSpan.FromDays(7);

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public void SaveRefreshToken(string token, int userId)
    {
        var tokenInfo = new RefreshTokenInfo
        {
            Token = token,
            UserId = userId,
            ExpirationDate = DateTime.UtcNow.Add(_refreshTokenExpiration)
        };

        _tokens.AddOrUpdate(token, tokenInfo, (_, _) => tokenInfo);
    }

    public RefreshTokenInfo? ValidateRefreshToken(string token)
    {
        if (_tokens.TryGetValue(token, out var tokenInfo))
        {
            // Si el token expiró, eliminarlo
            if (DateTime.UtcNow > tokenInfo.ExpirationDate)
            {
                _tokens.TryRemove(token, out _);
                return null;
            }

            return tokenInfo;
        }

        return null;
    }

    public void RevokeRefreshToken(string token)
    {
        _tokens.TryRemove(token, out _);
    }

    public void RevokeAllUserTokens(int userId)
    {
        var tokensToRemove = _tokens
            .Where(t => t.Value.UserId == userId)
            .Select(t => t.Key)
            .ToList();

        foreach (var token in tokensToRemove)
        {
            _tokens.TryRemove(token, out _);
        }
    }
}

public class RefreshTokenInfo
{
    public string Token { get; set; } = null!;
    public int UserId { get; set; }
    public DateTime ExpirationDate { get; set; }
}