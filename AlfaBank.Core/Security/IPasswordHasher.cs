namespace AlfaBank.Core.Security;

/// <summary>
/// Сервис формирования и проверки хэшей паролей.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Формирует хэш пароля с новой случайной солью.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <returns>Строка хэша вместе с параметрами алгоритма.</returns>
    string Hash(string password);

    /// <summary>
    /// Формирует хэш пароля с заданной солью. Используется только для демонстрационных данных,
    /// чтобы значения SeedData были детерминированными.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <param name="salt">Соль в кодировке Base64.</param>
    /// <returns>Строка хэша вместе с параметрами алгоритма.</returns>
    string Hash(string password, string salt);

    /// <summary>
    /// Проверяет соответствие пароля сохранённому хэшу.
    /// </summary>
    /// <param name="password">Проверяемый пароль.</param>
    /// <param name="passwordHash">Сохранённая строка хэша.</param>
    /// <returns>Значение <see langword="true"/>, если пароль верен.</returns>
    bool Verify(string password, string passwordHash);
}
