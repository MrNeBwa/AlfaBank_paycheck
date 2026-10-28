using AlfaBank.Core.Models.Enums;

namespace AlfaBank.App.ViewModels.Specialist;

public sealed record ApplicationStatus(CreditApplicationStatus? DomainStatus, string Title)
{
    public static ApplicationStatus All { get; } = new(null, "Все статусы");
}
