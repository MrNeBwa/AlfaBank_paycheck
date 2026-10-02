using AlfaBank.Core.Models.Enums;

namespace AlfaBank.App.ViewModels.Specialist;

/// <summary>
/// Пункт фильтра очереди заявок по статусу.
/// </summary>
/// <param name="DomainStatus">Значение статуса. Значение null означает отсутствие фильтра.</param>
/// <param name="Title">Название фильтра на русском языке.</param>
public sealed record ApplicationStatus(CreditApplicationStatus? DomainStatus, string Title)
{
    /// <summary>
    /// Фильтр, отображающий заявки всех статусов.
    /// </summary>
    public static ApplicationStatus All { get; } = new(null, "Все статусы");
}
