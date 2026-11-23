using AlfaBank.Core.Results;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

public interface IClientService
{
    Task<IReadOnlyList<ProductDto>> GetAvailableProductsAsync(CancellationToken cancellationToken = default);

    Task<OperationResult> SubmitApplicationAsync(
        int clientProfileId,
        ApplicationSubmissionDto submission,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationSummaryDto>> GetMyApplicationsAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default);

    Task<OperationResult<ClientOverviewDto>> GetOverviewAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CreditSummaryDto>> GetMyCreditsAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default);

    Task<OperationResult<IReadOnlyList<ScheduleRowDto>>> GetMyScheduleAsync(
        int clientProfileId,
        int creditId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentDto>> GetMyPaymentsAsync(
        int clientProfileId,
        CancellationToken cancellationToken = default);
}
