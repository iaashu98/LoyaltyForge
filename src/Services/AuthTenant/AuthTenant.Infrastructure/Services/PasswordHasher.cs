using AuthTenant.Application.Interfaces;
using System.Security.Cryptography;

namespace AuthTenant.Infrastructure.Services;

/// <summary>
/// BCrypt-style password hasher using PBKDF2.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public string HashPassword(string password)
    {
        using var algorithm = new Rfc2898DeriveBytes(
            password,
            SaltSize,
            Iterations,
            HashAlgorithmName.SHA256);

        var salt = algorithm.Salt;
        var hash = algorithm.GetBytes(HashSize);

        var result = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, result, SaltSize, HashSize);

        return Convert.ToBase64String(result);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var hashBytes = Convert.FromBase64String(hashedPassword);

        if (hashBytes.Length != SaltSize + HashSize)
            return false;

        var salt = new byte[SaltSize];
        Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);

        using var algorithm = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);

        var hash = algorithm.GetBytes(HashSize);

        for (int i = 0; i < HashSize; i++)
        {
            if (hashBytes[SaltSize + i] != hash[i])
                return false;
        }

        return true;
    }
}
