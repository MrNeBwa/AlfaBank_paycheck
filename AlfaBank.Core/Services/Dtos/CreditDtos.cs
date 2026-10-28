using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Services.Dtos;

public sealed record ScheduleRowDto(
    int Id,
    int Number,
    DateOnly DueDate,
    decimal PaymentAmount,
    decimal InterestAmount,
    decimal PrincipalAmount,
    decimal RemainingDebt,
    ScheduleItemStatus Status,
    string StatusText,
    DateOnly? PaidOn);

public sealed record ScheduleDraft(
    int Number,
    DateOnly DueDate,
    decimal PaymentAmount,
    decimal InterestAmount,
    decimal PrincipalAmount,
    decimal RemainingDebt);

public sealed record CreditSummaryDto(
    int Id,
    int ApplicationId,
    string ClientFullName,
    string ProductName,
    DateOnly IssuedOn,
    decimal Amount,
    decimal AnnualInterestRate,
    int TermMonths,
    CreditStatus Status,
    string StatusText,
    decimal PaidAmount,
    decimal RemainingDebt,
    int PaidPaymentsCount,
    int TotalPaymentsCount,
    DateOnly? NextPaymentDate,
    decimal NextPaymentAmount);

public sealed record PaymentDto(
    int Id,
    int CreditId,
    int CreditScheduleItemId,
    DateOnly PaidOn,
    decimal Amount,
    PaymentStatus Status,
    string StatusText,
    string Comment,
    string RegisteredByFullName);

public sealed record PaymentRegistrationDto(
    int CreditId,
    int CreditScheduleItemId,
    DateOnly PaidOn,
    decimal Amount,
    string Comment);

public sealed record ClientOverviewDto(
    int ApplicationsTotalCount,
    int ApplicationsNewCount,
    int ApplicationsApprovedCount,
    int ActiveCreditsCount,
    decimal RemainingDebtTotal,
    decimal PaidTotal);
