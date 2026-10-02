namespace AlfaBank.Core.Models.Enums;

/// <summary>
/// Состояние отдельного платежа в графике погашения кредита.
/// </summary>
public enum ScheduleItemStatus
{
    /// <summary>
    /// Платёж запланирован, но ещё не оплачен.
    /// </summary>
    Planned = 1,

    /// <summary>
    /// Платёж оплачен в установленный или более поздний срок.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Срок платежа истёк, а платёж не внесён.
    /// </summary>
    Overdue = 3
}
