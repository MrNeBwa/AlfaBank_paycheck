using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AlfaBank.Core.Security;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    public const string AlgorithmName = "PBKDF2-SHA256";

    public const int IterationsCount = 210_000;

    public const int SaltSizeInBytes = 16;

    public const int HashSizeInBytes = 32;

    private const int HashFormatPartsCount = 4;

    private const int IterationsPartIndex = 1;

    private const int SaltPartIndex = 2;

    private const int HashPartIndex = 3;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeInBytes);

        return Hash(password, Convert.ToBase64String(salt));
    }

    public string Hash(string password, string salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(salt);

        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = ComputeHash(password, saltBytes, IterationsCount);

        return string.Join(
            '$',
            AlgorithmName,
            IterationsCount.ToString(CultureInfo.InvariantCulture),
            salt,
            Convert.ToBase64String(hashBytes));
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split('$', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != HashFormatPartsCount || parts[0] != AlgorithmName)
        {
            return false;
        }

        if (!int.TryParse(parts[IterationsPartIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out var iterations))
        {
            return false;
        }

        byte[] saltBytes;
        byte[] expectedHash;

        try
        {
            saltBytes = Convert.FromBase64String(parts[SaltPartIndex]);
            expectedHash = Convert.FromBase64String(parts[HashPartIndex]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = ComputeHash(password, saltBytes, iterations);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static byte[] ComputeHash(string password, byte[] salt, int iterations)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        return Rfc2898DeriveBytes.Pbkdf2(passwordBytes, salt, iterations, HashAlgorithmName.SHA256, HashSizeInBytes);
    }
}
