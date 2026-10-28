using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Results;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

public interface ICreditSpecialistService
{
    Task<IReadOnlyList<ApplicationSummaryDto>> GetQueueAsync(
        CreditApplicationStatus? statusFilter,
        CancellationToken cancellationToken = default);

    Task<OperationResult<ApplicationDetailsDto>> GetApplicationDetailsAsync(
        int applicationId,
        CancellationToken cancellationToken = default);

    Task<OperationResult<ScoringResultDto>> EvaluateApplicationAsync(
        int applicationId,
        CancellationToken cancellationToken = default);

    Task<OperationResult<int>> ApproveApplicationAsync(
        int applicationId,
        int specialistUserId,
        string comment,
        CancellationToken cancellationToken = default);

    Task<OperationResult> RejectApplicationAsync(
        int applicationId,
        int specialistUserId,
        string comment,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CreditSummaryDto>> GetCreditsAsync(
        string searchText,
        CancellationToken cancellationToken = default);

    Task<OperationResult<IReadOnlyList<ScheduleRowDto>>> GetScheduleAsync(
        int creditId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> RegisterPaymentAsync(
        PaymentRegistrationDto registration,
        int specialistUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(
        int creditId,
        CancellationToken cancellationToken = default);
}
