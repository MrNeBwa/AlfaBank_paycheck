using AlfaBank.Core.Services.Dtos;

namespace AlfaBank.App.Infrastructure;

public sealed class UserSessionHolder : ObservableObject
{
    private UserSessionDto? _currentUser;

    public UserSessionDto? CurrentUser
    {
        get => _currentUser;
        private set => SetProperty(ref _currentUser, value);
    }

    public void SignIn(UserSessionDto user) => CurrentUser = user;

    public void SignOut() => CurrentUser = null;
}
