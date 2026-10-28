using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly UserSessionHolder _userSessionHolder;

    private string _login = string.Empty;
    private string _password = string.Empty;
    private bool _isSignedIn;

    public LoginViewModel(IAuthService authService, UserSessionHolder userSessionHolder)
    {
        _authService = authService;
        _userSessionHolder = userSessionHolder;

        SignInCommand = new AsyncRelayCommand(SignInAsync, onError: ReportUnexpectedError);
    }

    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool IsSignedIn
    {
        get => _isSignedIn;
        private set => SetProperty(ref _isSignedIn, value);
    }

    public AsyncRelayCommand SignInCommand { get; }

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
            ShowError(result.ErrorMessage);
            return;
        }

        _userSessionHolder.SignIn(result.Value!);
        IsSignedIn = true;
    }
}
