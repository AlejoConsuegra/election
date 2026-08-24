using System.Collections.Concurrent;

namespace Election.CRM.API.Common.Services;

public class LoginAttemptService
{
    private readonly ConcurrentDictionary<string, LoginAttemptInfo> _attempts = new();
    private const int MaxAttempts = 5;
    private const int LockoutMinutes = 15;

    public bool IsLockedOut(string ipAddress)
    {
        if (_attempts.TryGetValue(ipAddress, out var attempt))
        {
            // Si el bloqueo expiró, limpiar y permitir
            if (attempt.LockoutUntil.HasValue && DateTime.UtcNow > attempt.LockoutUntil.Value)
            {
                _attempts.TryRemove(ipAddress, out _);
                return false;
            }

            // Si está bloqueado y no ha expirado
            return attempt.LockoutUntil.HasValue && DateTime.UtcNow <= attempt.LockoutUntil.Value;
        }

        return false;
    }

    public void RecordFailedAttempt(string ipAddress)
    {
        var attempt = _attempts.GetOrAdd(ipAddress, _ => new LoginAttemptInfo());
        
        attempt.AttemptCount++;
        attempt.LastAttempt = DateTime.UtcNow;

        // Si alcanzó el máximo de intentos, bloquear
        if (attempt.AttemptCount >= MaxAttempts)
        {
            attempt.LockoutUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
        }
    }

    public void ResetAttempts(string ipAddress)
    {
        _attempts.TryRemove(ipAddress, out _);
    }

    public int GetRemainingAttempts(string ipAddress)
    {
        if (_attempts.TryGetValue(ipAddress, out var attempt))
        {
            return Math.Max(0, MaxAttempts - attempt.AttemptCount);
        }
        return MaxAttempts;
    }

    public DateTime? GetLockoutUntil(string ipAddress)
    {
        if (_attempts.TryGetValue(ipAddress, out var attempt))
        {
            return attempt.LockoutUntil;
        }
        return null;
    }
}

public class LoginAttemptInfo
{
    public int AttemptCount { get; set; }
    public DateTime LastAttempt { get; set; }
    public DateTime? LockoutUntil { get; set; }
}