namespace Election.CRM.API.Common.Helpers;

public static class PasswordHelper
{
    /// <summary>
    /// Hashea una contraseña en texto plano usando BCrypt
    /// </summary>
    public static string HashPassword(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }

    /// <summary>
    /// Verifica si una contraseña en texto plano coincide con un hash BCrypt
    /// </summary>
    public static bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }
}