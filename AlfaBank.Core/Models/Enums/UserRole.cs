namespace AlfaBank.Core.Models.Enums;

/// <summary>
/// Роль пользователя в системе автоматизации кредитных операций.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Клиент банка: подаёт заявки и отслеживает свои кредиты.
    /// </summary>
    Client = 1,

    /// <summary>
    /// Кредитный специалист: обрабатывает заявки и ведёт учёт платежей.
    /// </summary>
    CreditSpecialist = 2,

    /// <summary>
    /// Администратор системы: управляет пользователями и кредитными продуктами.
    /// </summary>
    Administrator = 3
}
