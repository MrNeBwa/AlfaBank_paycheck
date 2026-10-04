using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Services.Dtos;

/// <summary>
/// Данные, отправляемые клиентом при подаче заявки на кредит.
/// </summary>
/// <param name="CreditProductId">Идентификатор выбранного кредитного продукта.</param>
/// <param name="Amount">Запрашиваемая сумма кредита.</param>
/// <param name="TermMonths">Запрашиваемый срок кредита в месяцах.</param>
public sealed record ApplicationSubmissionDto(int CreditProductId, decimal Amount, int TermMonths);

/// <summary>
/// Краткая информация о заявке для списка.
/// </summary>
/// <param name="Id">Идентификатор заявки.</param>
/// <param name="ClientFullName">ФИО клиента.</param>
/// <param name="ProductName">Наименование кредитного продукта.</param>
/// <param name="Amount">Запрошенная сумма.</param>
/// <param name="TermMonths">Запрошенный срок в месяцах.</param>
/// <param name="Status">Статус заявки.</param>
/// <param name="StatusText">Название статуса на русском языке.</param>
/// <param name="MonthlyPayment">Расчётный ежемесячный платёж по аннуитету.</param>
/// <param name="DecisionComment">Комментарий специалиста по решению.</param>
/// <param name="CreatedAt">Дата подачи заявки.</param>
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

/// <summary>
/// Полная информация о заявке для проверки кредитным специалистом.
/// </summary>
/// <param name="Id">Идентификатор заявки.</param>
/// <param name="ClientFullName">ФИО клиента.</param>
/// <param name="PassportNumber">Паспорт клиента.</param>
/// <param name="BirthDate">Дата рождения клиента.</param>
/// <param name="AgeYears">Возраст клиента в полных годах.</param>
/// <param name="RegistrationAddress">Адрес регистрации клиента.</param>
/// <param name="EmployerName">Место работы клиента.</param>
/// <param name="EmploymentMonths">Стаж работы в месяцах.</param>
/// <param name="MonthlyIncome">Ежемесячный доход клиента.</param>
/// <param name="MonthlyExpenses">Ежемесячные расходы клиента.</param>
/// <param name="ProductName">Наименование кредитного продукта.</param>
/// <param name="AnnualInterestRate">Годовая процентная ставка по продукту.</param>
/// <param name="Amount">Запрошенная сумма кредита.</param>
/// <param name="TermMonths">Запрошенный срок кредита в месяцах.</param>
/// <param name="MonthlyPayment">Расчётный ежемесячный платёж.</param>
/// <param name="Status">Статус заявки.</param>
/// <param name="StatusText">Название статуса на русском языке.</param>
/// <param name="DecisionComment">Комментарий специалиста по решению.</param>
/// <param name="ScorePoints">Балльная оценка заёмщика после проверки.</param>
/// <param name="PaymentSharePercent">Доля платежа в доходе (ПДР) в процентах.</param>
/// <param name="CreatedAt">Дата подачи заявки.</param>
/// <param name="ReviewedAt">Дата рассмотрения заявки.</param>
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

/// <summary>
/// Исходные данные клиента для расчёта скоринга.
/// </summary>
/// <param name="AgeYears">Возраст заёмщика в полных годах.</param>
/// <param name="EmploymentMonths">Стаж работы в месяцах.</param>
/// <param name="MonthlyIncome">Ежемесячный доход заёмщика.</param>
/// <param name="MonthlyExpenses">Текущие ежемесячные расходы заёмщика.</param>
/// <param name="HasOverduePayments">Признак наличия просроченных платежей по действующим кредитам.</param>
public sealed record ScoringInput(
    int AgeYears,
    int EmploymentMonths,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    bool HasOverduePayments);

/// <summary>
/// Результат автоматической оценки заёмщика.
/// </summary>
/// <param name="ScorePoints">Итоговый балл от нуля до ста.</param>
/// <param name="PaymentSharePercent">Доля платежа в свободных средствах (ПДР) в процентах.</param>
/// <param name="DisposableIncome">Доход за вычетом текущих расходов, то есть свободные средства.</param>
/// <param name="MonthlyPayment">Расчётный ежемесячный платёж по кредиту.</param>
/// <param name="IsApproved">Признак соответствия заёмщика правилам выдачи кредита.</param>
/// <param name="Conclusion">Итоговый вывод по заёмщику на русском языке.</param>
/// <param name="Reasons">Список замечаний по результатам проверки.</param>
public sealed record ScoringResultDto(
    int ScorePoints,
    decimal PaymentSharePercent,
    decimal DisposableIncome,
    decimal MonthlyPayment,
    bool IsApproved,
    string Conclusion,
    IReadOnlyList<string> Reasons);
