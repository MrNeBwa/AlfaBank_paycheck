using AlfaBank.Core.Results;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

/// <summary>
/// Операции клиента: просмотр продуков, подача заявки, контроль кредитов и платежей.
/// </summary>
public interface IClientService
{
    /// <summary>
    /// Возвращает кредитные продукты, доступные для подачи заявки.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список доступных кредитных продуктов.</returns>
    Task<IReadOnlyList<ProductDto>> GetAvailableProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Подаёт заявку на кредит от имени клиента.
    /// </summary>
    /// <param name="clientProfileId">Идентификатор анкеты клиента.</param>
    /// <param name="submission">Данные заявки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Признак успеха либо текст ошибки.</returns>
    Task<OperationResult> SubmitApplicationAsync(
        int clientProfileId,
        ApplicationSubmissionDto submission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает заявки клиента с их статусами.
    /// </summary>
    /// <param name="clientProfileId">Идентификатор анкеты клиента.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список заявок клиента.</returns>
    Task<IReadOnlyList<ApplicationSummaryDto>> GetMyApplicationsAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает сводную информацию о кредитном портфеле клиента.
    /// </summary>
    /// <param name="clientProfileId">Идентификатор анкеты клиента.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Данные для главной страницы клиента.</returns>
    Task<OperationResult<ClientOverviewDto>> GetOverviewAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает кредиты клиента.
    /// </summary>
    /// <param name="clientProfileId">Идентификатор анкеты клиента.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список кредитов клиента.</returns>
    Task<IReadOnlyList<CreditSummaryDto>> GetMyCreditsAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает график платежей по кредиту клиента.
    /// </summary>
    /// <param name="clientProfileId">Идентификатор анкеты клиента.</param>
    /// <param name="creditId">Идентификатор кредита.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>График платежей по кредиту.</returns>
    Task<OperationResult<IReadOnlyList<ScheduleRowDto>>> GetMyScheduleAsync(
        int clientProfileId,
        int creditId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает платежи, внесённые клиентом по всем своим кредитам.
    /// </summary>
    /// <param name="clientProfileId">Идентификатор анкеты клиента.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список платежей клиента.</returns>
    Task<IReadOnlyList<PaymentDto>> GetMyPaymentsAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default);
}
