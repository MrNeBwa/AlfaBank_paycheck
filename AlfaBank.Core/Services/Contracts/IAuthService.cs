using AlfaBank.Core.Results;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

public interface IAuthService
{
    Task<OperationResult<UserSessionDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult<UserSessionDto>> RegisterClientAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken = default);
}
