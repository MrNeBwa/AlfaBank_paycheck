namespace AlfaBank.Core.Services.Dtos;

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
