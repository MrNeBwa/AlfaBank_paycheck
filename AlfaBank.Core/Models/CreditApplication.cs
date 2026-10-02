using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

/// <summary>
/// Заявка клиента на получение кредита.
/// </summary>
public class CreditApplication
{
    /// <summary>
    /// Идентификатор заявки.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор анкеты клиента, подавшего заявку.
    /// </summary>
    public int ClientProfileId { get; set; }

    /// <summary>
    /// Идентификатор запрошенного кредитного продукта.
    /// </summary>
    public int CreditProductId { get; set; }

    /// <summary>
    /// Запрошенная сумма кредита в рублях.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Запрошенный срок кредита в месяцах.
    /// </summary>
    public int TermMonths { get; set; }

    /// <summary>
    /// Текущий статус заявки.
    /// </summary>
    public CreditApplicationStatus Status { get; set; } = CreditApplicationStatus.New;

    /// <summary>
    /// Дата и время подачи заявки.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата и время рассмотрения заявки. Заполняется после решения специалиста.
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Идентификатор кредитного специалиста, принявшего решение.
    /// </summary>
    public int? ReviewedByUserId { get; set; }

    /// <summary>
    /// Комментарий специалиста с обоснованием решения.
    /// </summary>
    public string DecisionComment { get; set; } = string.Empty;

    /// <summary>
    /// Итоговый балл скоринга заёмщика (от 0 до 100). Заполняется после проверки.
    /// </summary>
    public int? ScorePoints { get; set; }

    /// <summary>
    /// Показатель долговой нагрузки (ПДР) в процентах. Заполняется после проверки.
    /// </summary>
    public decimal? PaymentSharePercent { get; set; }

    /// <summary>
    /// Анкета клиента — автор заявки.
    /// </summary>
    public ClientProfile ClientProfile { get; set; } = null!;

    /// <summary>
    /// Запрошенный кредитный продукт.
    /// </summary>
    public CreditProduct CreditProduct { get; set; } = null!;

    /// <summary>
    /// Кредитный специалист, принявший решение по заявке.
    /// </summary>
    public User? ReviewedBy { get; set; }

    /// <summary>
    /// Кредит, открытый по одобренной заявке.
    /// </summary>
    public Credit? Credit { get; set; }

    /// <summary>
    /// История изменения статуса заявки.
    /// </summary>
    public List<ApplicationStatusHistory> StatusHistory { get; set; } = [];
}
