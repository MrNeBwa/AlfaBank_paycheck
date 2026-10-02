using AlfaBank.Core.Models.Enums;

namespace AlfaBank.App.ViewModels.Admin;

/// <summary>
/// Роль сотрудника, доступная для назначения в учётной записи.
/// </summary>
/// <param name="Role">Значение роли.</param>
/// <param name="Title">Название роли на русском языке.</param>
public sealed record RoleOption(UserRole Role, string Title);