namespace AlfaBank.Core.Models;

/// <summary>
/// Анкета клиента: персональные и финансовые данные для оценки платёжеспособности.
/// </summary>
public class ClientProfile
{
    /// <summary>
    /// Идентификатор анкеты.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор учётной записи владельца анкеты.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Номер и серия паспорта клиента.
    /// </summary>
    public string PassportNumber { get; set; } = string.Empty;

    /// <summary>
    /// Дата рождения клиента.
    /// </summary>
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Адрес регистрации клиента.
    /// </summary>
    public string RegistrationAddress { get; set; } = string.Empty;

    /// <summary>
    /// Наименование места работы клиента.
    /// </summary>
    public string EmployerName { get; set; } = string.Empty;

    /// <summary>
    /// Стаж работы на текущем месте в месяцах.
    /// </summary>
    public int EmploymentMonths { get; set; }

    /// <summary>
    /// Ежемесячный доход клиента в рублях.
    /// </summary>
    public decimal MonthlyIncome { get; set; }

    /// <summary>
    /// Ежемесячные расходы клиента в рублях.
    /// </summary>
    public decimal MonthlyExpenses { get; set; }

    /// <summary>
    /// Учётная запись владельца анкеты.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Заявки клиента на получение кредита.
    /// </summary>
    public List<CreditApplication> Applications { get; set; } = [];
}
