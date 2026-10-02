using AlfaBank.Core.Results;
using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.Core.Services.Contracts;

/// <summary>
/// Регистрация пользователей, проверка учётных данных и вход в систему.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Выполняет вход в систему по логину и паролю.
    /// </summary>
    /// <param name="request">Данные для входа.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Данные текущего пользователя либо текст ошибки.</returns>
    Task<OperationResult<UserSessionDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Регистрирует нового клиента вместе с его анкетой.
    /// </summary>
    /// <param name="request">Данные регистрации клиента.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Данные текущего пользователя либо текст ошибки.</returns>
    Task<OperationResult<UserSessionDto>> RegisterClientAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken = default);
}
