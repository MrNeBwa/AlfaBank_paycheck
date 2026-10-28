using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Services.Dtos;

public sealed record ApplicationSubmissionDto(int CreditProductId, decimal Amount, int TermMonths);

public sealed record ApplicationSummaryDto(
    int Id,
    string ClientFullName,
    string ProductName,
    decimal Amount,
    int TermMonths,
    CreditApplicationStatus Status,
    string StatusText,
    decimal MonthlyPayment,
    string DecisionComment,
    DateTime CreatedAt);

public sealed record ApplicationDetailsDto(
    int Id,
    string ClientFullName,
    string PassportNumber,
    DateOnly BirthDate,
    int AgeYears,
    string RegistrationAddress,
    string EmployerName,
    int EmploymentMonths,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    string ProductName,
    decimal AnnualInterestRate,
    decimal Amount,
    int TermMonths,
    decimal MonthlyPayment,
    CreditApplicationStatus Status,
    string StatusText,
    string DecisionComment,
    int? ScorePoints,
    decimal? PaymentSharePercent,
    DateTime CreatedAt,
    DateTime? ReviewedAt);

public sealed record ScoringInput(
    int AgeYears,
    int EmploymentMonths,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    bool HasOverduePayments);

public sealed record ScoringResultDto(
    int ScorePoints,
    decimal PaymentSharePercent,
    decimal DisposableIncome,
    decimal MonthlyPayment,
    bool IsPaymentShareAcceptable,
    bool IsApproved,
    string Conclusion,
    IReadOnlyList<string> Reasons);
