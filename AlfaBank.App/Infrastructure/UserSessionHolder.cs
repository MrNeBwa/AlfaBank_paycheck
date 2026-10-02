using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Хранит данные текущего пользователя на время работы программы.
/// </summary>
public sealed class UserSessionHolder : ObservableObject
{
    private UserSessionDto? _currentUser;

    /// <summary>
    /// Текущий авторизованный пользователь. Значение null означает, что пользователь вышел из системы.
    /// </summary>
    public UserSessionDto? CurrentUser
    {
        get => _currentUser;
        private set => SetProperty(ref _currentUser, value);
    }

    /// <summary>
    /// Фиксирует вход пользователя в систему.
    /// </summary>
    /// <param name="user">Данные авторизованного пользователя.</param>
    public void SignIn(UserSessionDto user) => CurrentUser = user;

    /// <summary>
    /// Завершает сеанс пользователя.
    /// </summary>
    public void SignOut() => CurrentUser = null;
}
