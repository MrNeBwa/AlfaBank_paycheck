using AlfaBank.Core.Results;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

/// <summary>
/// Операции администратора: управление пользователями и справочником кредитных продуктов.
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Возвращает список пользователей системы.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список пользователей.</returns>
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Регистрирует нового сотрудника банка.
    /// </summary>
    /// <param name="registration">Данные сотрудника.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Идентификатор созданного пользователя либо текст ошибки.</returns>
    Task<OperationResult<int>> RegisterStaffAsync(
        StaffRegistrationDto registration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Изменяет данные учётной записи: имя, роль, активность и при необходимости пароль.
    /// </summary>
    /// <param name="editor">Новые данные учётной записи.</param>
    /// <param name="administratorUserId">Идентификатор администратора, выполняющего изменение.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат операции.</returns>
    Task<OperationResult> UpdateUserAsync(
        UserEditorDto editor,
        int administratorUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает справочник кредитных продуктов.
    /// </summary>
    /// <param name="onlyActive">Признак возврата только активных продуктов.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список кредитных продуктов.</returns>
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        bool onlyActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создаёт или изменяет кредитный продукт.
    /// </summary>
    /// <param name="editor">Данные продукта.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Идентификатор продукта либо текст ошибки.</returns>
    Task<OperationResult<int>> SaveProductAsync(
        ProductEditorDto editor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Включает или выключает кредитный продукт.
    /// </summary>
    /// <param name="productId">Идентификатор продукта.</param>
    /// <param name="isActive">Новый признак активности продукта.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат операции.</returns>
    Task<OperationResult> SetProductActivityAsync(
        int productId,
        bool isActive,
        CancellationToken cancellationToken = default);
}
