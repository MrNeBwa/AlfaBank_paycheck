using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Results;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

/// <summary>
/// Операции кредитного специалиста: обработка заявки, формирование графика и учёт платежей.
/// </summary>
public interface ICreditSpecialistService
{
    /// <summary>
    /// Возвращает очередь заявок с фильтром по статусу.
    /// </summary>
    /// <param name="statusFilter">Статус заявки или null для всех статусов.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список заявок.</returns>
    Task<IReadOnlyList<ApplicationSummaryDto>> GetQueueAsync(
        CreditApplicationStatus? statusFilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает подробные данные заявки для проверки.
    /// </summary>
    /// <param name="applicationId">Идентификатор заявки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Данные заявки либо текст ошибки.</returns>
    Task<OperationResult<ApplicationDetailsDto>> GetApplicationDetailsAsync(
        int applicationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполняет скоринг заёмщика по заявке без изменения данных.
    /// </summary>
    /// <param name="applicationId">Идентификатор заявки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат оценки заёмщика либо текст ошибки.</returns>
    Task<OperationResult<ScoringResultDto>> EvaluateApplicationAsync(
        int applicationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Одобряет заявку и формирует кредит с графиком платежей.
    /// </summary>
    /// <param name="applicationId">Идентификатор заявки.</param>
    /// <param name="specialistUserId">Идентификатор кредитного специалиста.</param>
    /// <param name="comment">Комментарий по решению.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Идентификатор открытого кредита либо текст ошибки.</returns>
    Task<OperationResult<int>> ApproveApplicationAsync(
        int applicationId,
        int specialistUserId,
        string comment,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отклоняет заявку с обязательным комментарием специалиста.
    /// </summary>
    /// <param name="applicationId">Идентификатор заявки.</param>
    /// <param name="specialistUserId">Идентификатор кредитного специалиста.</param>
    /// <param name="comment">Причина отказа.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат операции.</returns>
    Task<OperationResult> RejectApplicationAsync(
        int applicationId,
        int specialistUserId,
        string comment,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает портфель выданных кредитов.
    /// </summary>
    /// <param name="searchText">Фрагмент ФИО клиента или названия продукта. Пустое значение — все кредиты.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список кредитов.</returns>
    Task<IReadOnlyList<CreditSummaryDto>> GetCreditsAsync(
        string searchText,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает график платежей по кредиту.
    /// </summary>
    /// <param name="creditId">Идентификатор кредита.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>График платежей либо текст ошибки.</returns>
    Task<OperationResult<IReadOnlyList<ScheduleRowDto>>> GetScheduleAsync(
        int creditId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Регистрирует платёж по плану графика.
    /// </summary>
    /// <param name="registration">Данные платежа.</param>
    /// <param name="specialistUserId">Идентификатор кредитного специалиста.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат операции.</returns>
    Task<OperationResult> RegisterPaymentAsync(
        PaymentRegistrationDto registration,
        int specialistUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает платежи по кредиту.
    /// </summary>
    /// <param name="creditId">Идентификатор кредита.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список платежей по кредиту.</returns>
    Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(
        int creditId,
        CancellationToken cancellationToken = default);
}
