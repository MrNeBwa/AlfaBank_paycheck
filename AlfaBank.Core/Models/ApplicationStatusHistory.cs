using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Models;

/// <summary>
/// Запись об изменении статуса кредитной заявки.
/// </summary>
public class ApplicationStatusHistory
{
    /// <summary>
    /// Идентификатор записи истории.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор заявки.
    /// </summary>
    public int CreditApplicationId { get; set; }

    /// <summary>
    /// Статус заявки до изменения.
    /// </summary>
    public CreditApplicationStatus FromStatus { get; set; }

    /// <summary>
    /// Статус заявки после изменения.
    /// </summary>
    public CreditApplicationStatus ToStatus { get; set; }

    /// <summary>
    /// Дата и время изменения статуса.
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// Идентификатор пользователя, изменившего статус.
    /// </summary>
    public int ChangedByUserId { get; set; }

    /// <summary>
    /// Комментарий к изменению статуса.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Заявка, история которой хранится в записи.
    /// </summary>
    public CreditApplication CreditApplication { get; set; } = null!;

    /// <summary>
    /// Пользователь, изменивший статус заявки.
    /// </summary>
    public User ChangedBy { get; set; } = null!;
}
