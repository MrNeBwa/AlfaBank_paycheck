using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Services.Dtos;

/// <summary>
/// Плановый платёж графика погашения кредита.
/// </summary>
/// <param name="Id">Идентификатор платежа графика.</param>
/// <param name="Number">Порядковый номер платежа.</param>
/// <param name="DueDate">Дата планового платежа.</param>
/// <param name="PaymentAmount">Сумма платежа.</param>
/// <param name="InterestAmount">Сумма процентов за период.</param>
/// <param name="PrincipalAmount">Сумма погашения основного долга.</param>
/// <param name="RemainingDebt">Остаток задолженности после платежа.</param>
/// <param name="Status">Состояние платежа.</param>
/// <param name="StatusText">Название состояния на русском языке.</param>
/// <param name="PaidOn">Фактическая дата оплаты.</param>
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

/// <summary>
/// Плановый платёж, рассчитанный сервисом кредитного калькулятора.
/// </summary>
/// <param name="Number">Порядковый номер платежа.</param>
/// <param name="DueDate">Дата планового платежа.</param>
/// <param name="PaymentAmount">Сумма платежа.</param>
/// <param name="InterestAmount">Сумма процентов за период.</param>
/// <param name="PrincipalAmount">Сумма погашения основного долга.</param>
/// <param name="RemainingDebt">Остаток задолженности после платежа.</param>
public sealed record ScheduleDraft(
    int Number,
    DateOnly DueDate,
    decimal PaymentAmount,
    decimal InterestAmount,
    decimal PrincipalAmount,
    decimal RemainingDebt);

/// <summary>
/// Краткая информация о кредите для списков.
/// </summary>
/// <param name="Id">Идентификатор кредита.</param>
/// <param name="ApplicationId">Идентификатор заявки, по которой открыт кредит.</param>
/// <param name="ClientFullName">ФИО клиента.</param>
/// <param name="ProductName">Наименование кредитного продукта.</param>
/// <param name="IssuedOn">Дата выдачи кредита.</param>
/// <param name="Amount">Сумма кредита.</param>
/// <param name="AnnualInterestRate">Годовая процентная ставка.</param>
/// <param name="TermMonths">Срок кредита в месяцах.</param>
/// <param name="Status">Состояние кредита.</param>
/// <param name="StatusText">Название состояния на русском языке.</param>
/// <param name="PaidAmount">Сумма оплаченных платежей.</param>
/// <param name="RemainingDebt">Текущий остаток задолженности.</param>
/// <param name="PaidPaymentsCount">Количество оплаченных платежей.</param>
/// <param name="TotalPaymentsCount">Общее количество платежей графика.</param>
/// <param name="NextPaymentDate">Дата следующего платежа по графику.</param>
/// <param name="NextPaymentAmount">Сумма следующего платежа по графику.</param>
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

/// <summary>
/// Зарегистрированный платёж по кредиту.
/// </summary>
/// <param name="Id">Идентификатор платежа.</param>
/// <param name="CreditId">Идентификатор кредита.</param>
/// <param name="CreditScheduleItemId">Идентификатор оплаченного плана графика.</param>
/// <param name="PaidOn">Дата платежа.</param>
/// <param name="Amount">Сумма платежа.</param>
/// <param name="Status">Статус платежа.</param>
/// <param name="StatusText">Название статуса на русском языке.</param>
/// <param name="Comment">Комментарий специалиста.</param>
/// <param name="RegisteredByFullName">ФИО специалиста, зарегистрировавшего платёж.</param>
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

/// <summary>
/// Данные для регистрации платежа по графику.
/// </summary>
/// <param name="CreditId">Идентификатор кредита.</param>
/// <param name="CreditScheduleItemId">Идентификатор оплачиваемого плана графика.</param>
/// <param name="PaidOn">Дата фактического платежа.</param>
/// <param name="Amount">Сумма платежа.</param>
/// <param name="Comment">Комментарий специалиста.</param>
public sealed record PaymentRegistrationDto(
    int CreditId,
    int CreditScheduleItemId,
    DateOnly PaidOn,
    decimal Amount,
    string Comment);

/// <summary>
/// Сводная информация о кредитном портфеле клиента.
/// </summary>
/// <param name="ApplicationsTotalCount">Общее количество заявок клиента.</param>
/// <param name="ApplicationsNewCount">Количество заявок в работе.</param>
/// <param name="ApplicationsApprovedCount">Количество одобренных заявок.</param>
/// <param name="ActiveCreditsCount">Количество действующих кредитов.</param>
/// <param name="RemainingDebtTotal">Суммарный остаток задолженности клиента.</param>
/// <param name="PaidTotal">Суммарная сумма внесённых платежей клиента.</param>
public sealed record ClientOverviewDto(
    int ApplicationsTotalCount,
    int ApplicationsNewCount,
    int ApplicationsApprovedCount,
    int ActiveCreditsCount,
    decimal RemainingDebtTotal,
    decimal PaidTotal);
