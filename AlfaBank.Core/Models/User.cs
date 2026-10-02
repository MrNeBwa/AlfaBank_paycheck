using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

/// <summary>
/// Учётная запись пользователя системы.
/// </summary>
public class User
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Логин пользователя (уникальное значение).
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Хэш пароля пользователя в формате PBKDF2 (в открытом виде пароль не хранится).
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Полное имя пользователя.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя в системе.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Признак активной учётной записи. Неактивные пользователи не могут войти в систему.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Дата и время регистрации учётной записи.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Анкета клиента. Заполняется только для пользователей с ролью «Клиент».
    /// </summary>
    public ClientProfile? ClientProfile { get; set; }
}
