using System.Globalization;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels;

/// <summary>
/// Модель представления самостоятельной регистрации клиента в банке.
/// </summary>
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

    /// <summary>
    /// Создаёт модель представления регистрации клиента.
    /// </summary>
    /// <param name="authService">Сервис авторизации.</param>
    /// <param name="userSessionHolder">Хранилище данных текущего пользователя.</param>
    public RegisterClientViewModel(IAuthService authService, UserSessionHolder userSessionHolder)
    {
        _authService = authService;
        _userSessionHolder = userSessionHolder;

        RegisterCommand = new AsyncRelayCommand(RegisterAsync, onError: ReportUnexpectedError);
    }

    /// <summary>
    /// Придумываемый логин клиента.
    /// </summary>
    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    /// <summary>
    /// Придумываемый пароль клиента.
    /// </summary>
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    /// <summary>
    /// Полное имя клиента.
    /// </summary>
    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    /// <summary>
    /// Номер и серия паспорта клиента.
    /// </summary>
    public string PassportNumber
    {
        get => _passportNumber;
        set => SetProperty(ref _passportNumber, value);
    }

    /// <summary>
    /// Дата рождения клиента в формате ДД.ММ.ГГГГ.
    /// </summary>
    public string BirthDateText
    {
        get => _birthDateText;
        set => SetProperty(ref _birthDateText, value);
    }

    /// <summary>
    /// Адрес регистрации клиента.
    /// </summary>
    public string RegistrationAddress
    {
        get => _registrationAddress;
        set => SetProperty(ref _registrationAddress, value);
    }

    /// <summary>
    /// Место работы клиента.
    /// </summary>
    public string EmployerName
    {
        get => _employerName;
        set => SetProperty(ref _employerName, value);
    }

    /// <summary>
    /// Стаж работы в месяцах.
    /// </summary>
    public string EmploymentMonthsText
    {
        get => _employmentMonthsText;
        set => SetProperty(ref _employmentMonthsText, value);
    }

    /// <summary>
    /// Ежемесячный доход клиента в рублях.
    /// </summary>
    public string MonthlyIncomeText
    {
        get => _monthlyIncomeText;
        set => SetProperty(ref _monthlyIncomeText, value);
    }

    /// <summary>
    /// Ежемесячные расходы клиента в рублях.
    /// </summary>
    public string MonthlyExpensesText
    {
        get => _monthlyExpensesText;
        set => SetProperty(ref _monthlyExpensesText, value);
    }

    /// <summary>
    /// Признак успешной регистрации клиента.
    /// </summary>
    public bool IsRegistered
    {
        get => _isRegistered;
        private set => SetProperty(ref _isRegistered, value);
    }

    /// <summary>
    /// Команда регистрации клиента и создания его анкеты.
    /// </summary>
    public AsyncRelayCommand RegisterCommand { get; }

    /// <summary>
    /// Регистрирует клиента, создаёт его анкету и выполняет вход в систему.
    /// </summary>
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
            StatusMessage = result.ErrorMessage;
            return;
        }

        _userSessionHolder.SignIn(result.Value);
        IsRegistered = true;
    }

    /// <summary>
    /// Разбирает дату, введённую в формате ДД.ММ.ГГГГ.
    /// </summary>
    /// <param name="text">Введённое значение даты.</param>
    /// <returns>Дата или значение по умолчанию при ошибке ввода.</returns>
    private static DateOnly ParseDate(string text) =>
        DateOnly.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Разбирает целое число, введённое пользователем.
    /// </summary>
    /// <param name="text">Введённое значение.</param>
    /// <returns>Число или ноль при ошибке ввода.</returns>
    private static int ParseNumber(string text) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : 0;

    /// <summary>
    /// Разбирает денежную сумму, введённую пользователем.
    /// </summary>
    /// <param name="text">Введённое значение.</param>
    /// <returns>Сумма или ноль при ошибке ввода.</returns>
    private static decimal ParseAmount(string text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : 0m;
}