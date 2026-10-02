using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AlfaBank.Core.Security;

/// <summary>
/// Реализация хэширования паролей по стандарту PBKDF2 (HMAC-SHA256) со случайной солью.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Наименование алгоритма, сохраняемое в начале строки хэша.
    /// </summary>
    public const string AlgorithmName = "PBKDF2-SHA256";

    /// <summary>
    /// Количество итераций вычисления хэша.
    /// </summary>
    public const int IterationsCount = 210_000;

    /// <summary>
    /// Размер соли в байтах.
    /// </summary>
    public const int SaltSizeInBytes = 16;

    /// <summary>
    /// Размер хэша в байтах.
    /// </summary>
    public const int HashSizeInBytes = 32;

    /// <summary>
    /// Количество частей в сохраняемой строке хэша.
    /// </summary>
    private const int HashFormatPartsCount = 4;

    /// <summary>
    /// Индекс части, содержащей количество итераций.
    /// </summary>
    private const int IterationsPartIndex = 1;

    /// <summary>
    /// Индекс части, содержащей соль в кодировке Base64.
    /// </summary>
    private const int SaltPartIndex = 2;

    /// <summary>
    /// Индекс части, содержащей хэш в кодировке Base64.
    /// </summary>
    private const int HashPartIndex = 3;

    /// <inheritdoc />
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeInBytes);

        return Hash(password, Convert.ToBase64String(salt));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <summary>
    /// Вычисляет хэш пароля для заданной соли и количества итераций.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <param name="salt">Соль в байтах.</param>
    /// <param name="iterations">Количество итераций.</param>
    /// <returns>Массив байтов хэша.</returns>
    private static byte[] ComputeHash(string password, byte[] salt, int iterations)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        return Rfc2898DeriveBytes.Pbkdf2(passwordBytes, salt, iterations, HashAlgorithmName.SHA256, HashSizeInBytes);
    }
}
