using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels;

/// <summary>
/// Модель представления окна входа в систему.
/// </summary>
public sealed class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly UserSessionHolder _userSessionHolder;

    private string _login = string.Empty;
    private string _password = string.Empty;
    private bool _isSignedIn;

    /// <summary>
    /// Создаёт модель представления окна входа.
    /// </summary>
    /// <param name="authService">Сервис авторизации.</param>
    /// <param name="userSessionHolder">Хранилище данных текущего пользователя.</param>
    public LoginViewModel(IAuthService authService, UserSessionHolder userSessionHolder)
    {
        _authService = authService;
        _userSessionHolder = userSessionHolder;

        SignInCommand = new AsyncRelayCommand(SignInAsync, onError: ReportUnexpectedError);
    }

    /// <summary>
    /// Логин пользователя.
    /// </summary>
    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    /// <summary>
    /// Признак успешного входа в систему.
    /// </summary>
    public bool IsSignedIn
    {
        get => _isSignedIn;
        private set => SetProperty(ref _isSignedIn, value);
    }

    /// <summary>
    /// Команда проверки учётных данных и входа в систему.
    /// </summary>
    public AsyncRelayCommand SignInCommand { get; }

    /// <summary>
    /// Проверяет учётные данные и фиксирует сеанс пользователя.
    /// </summary>
    private async Task SignInAsync()
    {
        var result = await ExecuteGuardedAsync(() =>
            _authService.LoginAsync(new LoginRequest(Login.Trim(), Password)));

        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            StatusMessage = result.ErrorMessage;
            return;
        }

        _userSessionHolder.SignIn(result.Value!);
        IsSignedIn = true;
    }
}
