using AlfaBank.Core.Data;
using AlfaBank.Core.Models;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Results;
using AlfaBank.Core.Security;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlfaBank.Core.Services;

/// <summary>
/// Сервис авторизации: регистрация клиентов, проверка паролей и вход в систему.
/// </summary>
public sealed class AuthService : IAuthService
{
    private const string LoginAlreadyUsedMessage = "Пользователь с таким логином уже зарегистрирован.";
    private const string InvalidCredentialsMessage = "Неверный логин или пароль.";
    private const string InactiveAccountMessage = "Учётная запись отключена. Обратитесь к администратору.";

    private readonly Func<AppDbContext> _contextFactory;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Создаёт сервис авторизации.
    /// </summary>
    /// <param name="contextFactory">Фабрика контекстов базы данных.</param>
    /// <param name="passwordHasher">Сервис хэширования паролей.</param>
    public AuthService(Func<AppDbContext> contextFactory, IPasswordHasher passwordHasher)
    {
        _contextFactory = contextFactory;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc />
    public async Task<OperationResult<UserSessionDto>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationMessage = ValidateLoginRequest(request);

        if (validationMessage is not null)
        {
            return OperationResult<UserSessionDto>.Failure(validationMessage);
        }

        await using var context = _contextFactory();

        var user = await GetUserByLoginAsync(context, request.Login, cancellationToken);

        return BuildLoginResult(user, request.Password);
    }

    /// <inheritdoc />
    public async Task<OperationResult<UserSessionDto>> RegisterClientAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationMessage = ValidateRegistrationRequest(request);

        if (validationMessage is not null)
        {
            return OperationResult<UserSessionDto>.Failure(validationMessage);
        }

        await using var context = _contextFactory();

        var isLoginTaken = await context.Users.AnyAsync(
            user => user.Login == request.Login,
            cancellationToken);

        if (isLoginTaken)
        {
            return OperationResult<UserSessionDto>.Failure(LoginAlreadyUsedMessage);
        }

        var user = new User
        {
            Login = request.Login,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Role = UserRole.Client,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        user.ClientProfile = CreateProfile(request);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return OperationResult<UserSessionDto>.Success(CreateSession(user));
    }

    /// <summary>
    /// Формирует результат входа по найденной учётной записи.
    /// </summary>
    /// <param name="user">Найденный пользователь или null.</param>
    /// <param name="password">Проверяемый пароль.</param>
    /// <returns>Данные сеанса либо текст ошибки.</returns>
    private OperationResult<UserSessionDto> BuildLoginResult(User? user, string password)
    {
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            return OperationResult<UserSessionDto>.Failure(InvalidCredentialsMessage);
        }

        if (!user.IsActive)
        {
            return OperationResult<UserSessionDto>.Failure(InactiveAccountMessage);
        }

        return OperationResult<UserSessionDto>.Success(CreateSession(user));
    }

    /// <summary>
    /// Создаёт анкету клиента по данным регистрации.
    /// </summary>
    /// <param name="request">Данные регистрации клиента.</param>
    /// <returns>Новая анкета клиента.</returns>
    private static ClientProfile CreateProfile(RegistrationRequest request) => new()
    {
        PassportNumber = request.PassportNumber,
        BirthDate = request.BirthDate,
        RegistrationAddress = request.RegistrationAddress,
        EmployerName = request.EmployerName,
        EmploymentMonths = request.EmploymentMonths,
        MonthlyIncome = request.MonthlyIncome,
        MonthlyExpenses = request.MonthlyExpenses
    };

    /// <summary>
    /// Формирует данные сеанса пользователя.
    /// </summary>
    /// <param name="user">Пользователь с загруженной анкетой.</param>
    /// <returns>Данные текущего пользователя.</returns>
    private static UserSessionDto CreateSession(User user) => new(
        user.Id,
        user.Login,
        user.FullName,
        user.Role,
        StatusTextProvider.GetRoleText(user.Role),
        user.ClientProfile?.Id);

    /// <summary>
    /// Ищет пользователя вместе с его анкетой по логину.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="login">Логин пользователя.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Найденный пользователь или null.</returns>
    private static Task<User?> GetUserByLoginAsync(AppDbContext context, string login, CancellationToken cancellationToken) =>
        context.Users
            .Include(user => user.ClientProfile)
            .FirstOrDefaultAsync(user => user.Login == login, cancellationToken);

    /// <summary>
    /// Проверяет данные для входа в систему.
    /// </summary>
    /// <param name="request">Данные для входа.</param>
    /// <returns>Текст ошибки или null, если данные корректны.</returns>
    private static string? ValidateLoginRequest(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login))
        {
            return "Введите логин.";
        }

        return string.IsNullOrWhiteSpace(request.Password) ? "Введите пароль." : null;
    }

    /// <summary>
    /// Проверяет данные самостоятельной регистрации клиента.
    /// </summary>
    /// <param name="request">Данные регистрации.</param>
    /// <returns>Текст ошибки или null, если данные корректны.</returns>
    private static string? ValidateRegistrationRequest(RegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login))
        {
            return "Введите логин.";
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < BankConstants.PasswordMinimumLength)
        {
            return $"Пароль должен содержать не менее {BankConstants.PasswordMinimumLength} символов.";
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return "Укажите фамилию, имя и отчество.";
        }

        if (string.IsNullOrWhiteSpace(request.PassportNumber))
        {
            return "Укажите номер и серию паспорта.";
        }

        if (request.BirthDate > DateOnly.FromDateTime(DateTime.Today))
        {
            return "Дата рождения указана неверно.";
        }

        if (string.IsNullOrWhiteSpace(request.EmployerName))
        {
            return "Укажите место работы.";
        }

        if (request.EmploymentMonths < 0)
        {
            return "Стаж работы не может быть отрицательным.";
        }

        if (request.MonthlyExpenses < 0m)
        {
            return "Ежемесячные расходы не могут быть отрицательными.";
        }

        return request.MonthlyIncome > 0m ? null : "Укажите ежемесячный доход.";
    }
}
