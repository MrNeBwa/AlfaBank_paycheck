namespace AlfaBank.Core.Models;

/// <summary>
/// Кредитный продукт из справочника банка.
/// </summary>
public class CreditProduct
{
    /// <summary>
    /// Идентификатор кредитного продукта.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Наименование кредитного продукта.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Годовая процентная ставка в процентах.
    /// </summary>
    public decimal AnnualInterestRate { get; set; }

    /// <summary>
    /// Минимальная сумма кредита в рублях.
    /// </summary>
    public decimal MinAmount { get; set; }

    /// <summary>
    /// Максимальная сумма кредита в рублях.
    /// </summary>
    public decimal MaxAmount { get; set; }

    /// <summary>
    /// Минимальный срок кредита в месяцах.
    /// </summary>
    public int MinTermMonths { get; set; }

    /// <summary>
    /// Максимальный срок кредита в месяцах.
    /// </summary>
    public int MaxTermMonths { get; set; }

    /// <summary>
    /// Пояснение к продукту, отображаемое клиенту при подаче заявки.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Признак доступности продукта для новых заявок.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Дата и время добавления продукта в справочник.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Заявки, оформленные по данному продукту.
    /// </summary>
    public List<CreditApplication> Applications { get; set; } = [];
}
