using AlfaBank.Core.Results;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

public interface IAdminService
{
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<OperationResult<int>> RegisterStaffAsync(
        StaffRegistrationDto registration,
        CancellationToken cancellationToken = default);

    Task<OperationResult> UpdateUserAsync(
        UserEditorDto editor,
        int administratorUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        bool onlyActive,
        CancellationToken cancellationToken = default);

    Task<OperationResult<int>> SaveProductAsync(
        ProductEditorDto editor,
        CancellationToken cancellationToken = default);

    Task<OperationResult> SetProductActivityAsync(
        int productId,
        bool isActive,
        CancellationToken cancellationToken = default);
}
