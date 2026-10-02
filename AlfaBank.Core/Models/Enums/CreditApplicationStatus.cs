namespace AlfaBank.Core.Models.Enums;

/// <summary>
/// Статус кредитной заявки на каждом этапе обработки.
/// </summary>
public enum CreditApplicationStatus
{
    /// <summary>
    /// Заявка подана и ожидает рассмотрения кредитным специалистом.
    /// </summary>
    New = 1,

    /// <summary>
    /// Заявка одобрена, по ней открыт кредит с графиком платежей.
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Заявка отклонена кредитным специалистом.
    /// </summary>
    Rejected = 3
}
