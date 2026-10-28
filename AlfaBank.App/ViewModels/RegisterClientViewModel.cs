using System.Globalization;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels;

public sealed class RegisterClientViewModel : ViewModelBase
{
    private const string DateFormat = "dd.MM.yyyy";

    private readonly IAuthService _authService;
    private readonly UserSessionHolder _userSessionHolder;

    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _fullName = string.Empty;
    private string _passportNumber = string.Empty;
    private string _birthDateText = string.Empty;
    private string _registrationAddress = string.Empty;
    private string _employerName = string.Empty;
    private string _employmentMonthsText = string.Empty;
    private string _monthlyIncomeText = string.Empty;
    private string _monthlyExpensesText = string.Empty;
    private bool _isRegistered;

    public RegisterClientViewModel(IAuthService authService, UserSessionHolder userSessionHolder)
    {
        _authService = authService;
        _userSessionHolder = userSessionHolder;

        RegisterCommand = new AsyncRelayCommand(RegisterAsync, onError: ReportUnexpectedError);
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

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string PassportNumber
    {
        get => _passportNumber;
        set => SetProperty(ref _passportNumber, value);
    }

    public string BirthDateText
    {
        get => _birthDateText;
        set => SetProperty(ref _birthDateText, value);
    }

    public string RegistrationAddress
    {
        get => _registrationAddress;
        set => SetProperty(ref _registrationAddress, value);
    }

    public string EmployerName
    {
        get => _employerName;
        set => SetProperty(ref _employerName, value);
    }

    public string EmploymentMonthsText
    {
        get => _employmentMonthsText;
        set => SetProperty(ref _employmentMonthsText, value);
    }

    public string MonthlyIncomeText
    {
        get => _monthlyIncomeText;
        set => SetProperty(ref _monthlyIncomeText, value);
    }

    public string MonthlyExpensesText
    {
        get => _monthlyExpensesText;
        set => SetProperty(ref _monthlyExpensesText, value);
    }

    public bool IsRegistered
    {
        get => _isRegistered;
        private set => SetProperty(ref _isRegistered, value);
    }

    public AsyncRelayCommand RegisterCommand { get; }

    private async Task RegisterAsync()
    {
        var request = new RegistrationRequest(
            Login.Trim(),
            Password,
            FullName.Trim(),
            PassportNumber.Trim(),
            ParseDate(BirthDateText),
            RegistrationAddress.Trim(),
            EmployerName.Trim(),
            ParseNumber(EmploymentMonthsText),
            ParseAmount(MonthlyIncomeText),
            ParseAmount(MonthlyExpensesText));

        var result = await ExecuteGuardedAsync(() => _authService.RegisterClientAsync(request));

        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess || result.Value is null)
        {
            ShowError(result.ErrorMessage);
            return;
        }

        _userSessionHolder.SignIn(result.Value);
        IsRegistered = true;
    }

    private static DateOnly ParseDate(string text) =>
        DateOnly.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : DateOnly.FromDateTime(DateTime.Today);

    private static int ParseNumber(string text) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : 0;

    private static decimal ParseAmount(string text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : 0m;
}