namespace AlfaBank.Core.Services.Dtos;

/// <summary>
/// Данные кредитного продукта для интерфейса программы.
/// </summary>
/// <param name="Id">Идентификатор продукта.</param>
/// <param name="Name">Наименование продукта.</param>
/// <param name="AnnualInterestRate">Годовая процентная ставка.</param>
/// <param name="MinAmount">Минимальная сумма кредита.</param>
/// <param name="MaxAmount">Максимальная сумма кредита.</param>
/// <param name="MinTermMonths">Минимальный срок кредита в месяцах.</param>
/// <param name="MaxTermMonths">Максимальный срок кредита в месяцах.</param>
/// <param name="Description">Пояснение к продукту.</param>
/// <param name="IsActive">Признак доступности продукта для новых заявок.</param>
public sealed record ProductDto(
    int Id,
    string Name,
    decimal AnnualInterestRate,
    decimal MinAmount,
    decimal MaxAmount,
    int MinTermMonths,
    int MaxTermMonths,
    string Description,
    bool IsActive);

/// <summary>
/// Данные кредитного продукта для создания и изменения в справочнике.
/// </summary>
/// <param name="Id">Идентификатор продукта. Ноль означает создание нового продукта.</param>
/// <param name="Name">Наименование продукта.</param>
/// <param name="AnnualInterestRate">Годовая процентная ставка.</param>
/// <param name="MinAmount">Минимальная сумма кредита.</param>
/// <param name="MaxAmount">Максимальная сумма кредита.</param>
/// <param name="MinTermMonths">Минимальный срок кредита в месяцах.</param>
/// <param name="MaxTermMonths">Максимальный срок кредита в месяцах.</param>
/// <param name="Description">Пояснение к продукту.</param>
/// <param name="IsActive">Признак доступности продукта.</param>
public sealed record ProductEditorDto(
    int Id,
    string Name,
    decimal AnnualInterestRate,
    decimal MinAmount,
    decimal MaxAmount,
    int MinTermMonths,
    int MaxTermMonths,
    string Description,
    bool IsActive);
