using AlfaBank.Core.Data;
using AlfaBank.Core.Models;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Results;
using AlfaBank.Core.Security;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AlfaBank.Core.Services;

public sealed class AdminService : IAdminService
{
    private const string LoginAlreadyUsedMessage = "Пользователь с таким логином уже зарегистрирован.";
    private const string UserNotFoundMessage = "Пользователь не найден.";
    private const string ProductNotFoundMessage = "Кредитный продукт не найден.";
    private const string SelfDeactivationMessage = "Нельзя отключить собственную учётную запись.";
    private const string SelfRoleChangeMessage = "Нельзя изменить собственную роль.";

    private readonly Func<AppDbContext> _contextFactory;
    private readonly IPasswordHasher _passwordHasher;

    public AdminService(Func<AppDbContext> contextFactory, IPasswordHasher passwordHasher)
    {
        _contextFactory = contextFactory;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var users = await context.Users
            .Include(user => user.ClientProfile)
            .OrderBy(user => user.FullName)
            .ToListAsync(cancellationToken);

        return users.Select(EntityMapper.ToUserDto).ToList();
    }

    public async Task<OperationResult<int>> RegisterStaffAsync(
        StaffRegistrationDto registration,
        CancellationToken cancellationToken = default)
    {
        var validationMessage = ValidateStaffRegistration(registration);

        if (validationMessage is not null)
        {
            return OperationResult<int>.Failure(validationMessage);
        }

        await using var context = _contextFactory();

        var isLoginTaken = await context.Users.AnyAsync(
            user => user.Login == registration.Login,
            cancellationToken);

        if (isLoginTaken)
        {
            return OperationResult<int>.Failure(LoginAlreadyUsedMessage);
        }

        var user = new User
        {
            Login = registration.Login,
            PasswordHash = _passwordHasher.Hash(registration.Password),
            FullName = registration.FullName,
            Role = registration.Role,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        if (registration.Role == UserRole.Client)
        {
            user.ClientProfile = CreateProfile(registration.Profile!);
        }

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Success(user.Id);
    }

    public async Task<OperationResult> UpdateUserAsync(
        UserEditorDto editor,
        int administratorUserId,
        CancellationToken cancellationToken = default)
    {
        var validationMessage = ValidateUserEditor(editor, administratorUserId);

        if (validationMessage is not null)
        {
            return OperationResult.Failure(validationMessage);
        }

        await using var context = _contextFactory();

        var user = await context.Users
            .Include(candidate => candidate.ClientProfile)
            .FirstOrDefaultAsync(candidate => candidate.Id == editor.UserId, cancellationToken);

        if (user is null)
        {
            return OperationResult.Failure(UserNotFoundMessage);
        }

        if (editor.Role == UserRole.Client)
        {
            var profileValidationMessage = ValidateClientProfile(editor.Profile ?? ToProfileInput(user.ClientProfile));

            if (profileValidationMessage is not null)
            {
                return OperationResult.Failure(profileValidationMessage);
            }

            if (user.ClientProfile is null)
            {
                user.ClientProfile = CreateProfile(editor.Profile!);
            }
            else
            {
                ApplyProfileChanges(user.ClientProfile, editor.Profile!);
            }
        }

        var newLogin = editor.Login?.Trim() ?? string.Empty;

        if (newLogin.Length > 0 && !string.Equals(newLogin, user.Login, StringComparison.Ordinal))
        {
            var isLoginTaken = await context.Users.AnyAsync(
                candidate => candidate.Login == newLogin && candidate.Id != editor.UserId,
                cancellationToken);

            if (isLoginTaken)
            {
                return OperationResult.Failure(LoginAlreadyUsedMessage);
            }

            user.Login = newLogin;
        }

        ApplyUserChanges(user, editor);

        await context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(
        bool onlyActive,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var query = context.CreditProducts.AsQueryable();

        if (onlyActive)
        {
            query = query.Where(product => product.IsActive);
        }

        var products = await query
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return products.Select(EntityMapper.ToProductDto).ToList();
    }

    public async Task<OperationResult<int>> SaveProductAsync(
        ProductEditorDto editor,
        CancellationToken cancellationToken = default)
    {
        var validationMessage = ValidateProduct(editor);

        if (validationMessage is not null)
        {
            return OperationResult<int>.Failure(validationMessage);
        }

        await using var context = _contextFactory();

        var product = await GetProductToEditAsync(context, editor.Id, cancellationToken);

        if (editor.Id != 0 && product is null)
        {
            return OperationResult<int>.Failure(ProductNotFoundMessage);
        }

        if (product is null)
        {
            product = new CreditProduct { CreatedAt = DateTime.Now };
            context.CreditProducts.Add(product);
        }

        ApplyProductChanges(product, editor);

        await context.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Success(product.Id);
    }

    public async Task<OperationResult> SetProductActivityAsync(
        int productId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var product = await context.CreditProducts
            .FirstOrDefaultAsync(candidate => candidate.Id == productId, cancellationToken);

        if (product is null)
        {
            return OperationResult.Failure(ProductNotFoundMessage);
        }

        product.IsActive = isActive;

        await context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }

    private static Task<CreditProduct?> GetProductToEditAsync(
        AppDbContext context,
        int productId,
        CancellationToken cancellationToken) =>
        context.CreditProducts.FirstOrDefaultAsync(product => product.Id == productId, cancellationToken);

    private static ClientProfile CreateProfile(ClientProfileInputDto profile) => new()
    {
        PassportNumber = profile.PassportNumber.Trim(),
        BirthDate = profile.BirthDate,
        RegistrationAddress = profile.RegistrationAddress.Trim(),
        EmployerName = profile.EmployerName.Trim(),
        EmploymentMonths = profile.EmploymentMonths,
        MonthlyIncome = profile.MonthlyIncome,
        MonthlyExpenses = profile.MonthlyExpenses
    };

    private static void ApplyProfileChanges(ClientProfile profile, ClientProfileInputDto input)
    {
        profile.PassportNumber = input.PassportNumber.Trim();
        profile.BirthDate = input.BirthDate;
        profile.RegistrationAddress = input.RegistrationAddress.Trim();
        profile.EmployerName = input.EmployerName.Trim();
        profile.EmploymentMonths = input.EmploymentMonths;
        profile.MonthlyIncome = input.MonthlyIncome;
        profile.MonthlyExpenses = input.MonthlyExpenses;
    }

    private static ClientProfileInputDto ToProfileInput(ClientProfile? profile) => new(
        profile?.PassportNumber ?? string.Empty,
        profile?.BirthDate ?? default,
        profile?.RegistrationAddress ?? string.Empty,
        profile?.EmployerName ?? string.Empty,
        profile?.EmploymentMonths ?? 0,
        profile?.MonthlyIncome ?? 0m,
        profile?.MonthlyExpenses ?? 0m);

    private void ApplyUserChanges(User user, UserEditorDto editor)
    {
        user.FullName = editor.FullName;
        user.Role = editor.Role;
        user.IsActive = editor.IsActive;

        if (!string.IsNullOrWhiteSpace(editor.NewPassword))
        {
            user.PasswordHash = _passwordHasher.Hash(editor.NewPassword);
        }
    }

    private static void ApplyProductChanges(CreditProduct product, ProductEditorDto editor)
    {
        product.Name = editor.Name;
        product.AnnualInterestRate = editor.AnnualInterestRate;
        product.MinAmount = editor.MinAmount;
        product.MaxAmount = editor.MaxAmount;
        product.MinTermMonths = editor.MinTermMonths;
        product.MaxTermMonths = editor.MaxTermMonths;
        product.Description = editor.Description;
        product.IsActive = editor.IsActive;
    }

    private static string? ValidateStaffRegistration(StaffRegistrationDto registration)
    {
        if (string.IsNullOrWhiteSpace(registration.Login))
        {
            return "Введите логин пользователя.";
        }

        if (string.IsNullOrWhiteSpace(registration.Password) || registration.Password.Length < BankConstants.PasswordMinimumLength)
        {
            return $"Пароль должен содержать не менее {BankConstants.PasswordMinimumLength} символов.";
        }

        if (string.IsNullOrWhiteSpace(registration.FullName))
        {
            return "Укажите фамилию, имя и отчество пользователя.";
        }

        return registration.Role == UserRole.Client
            ? ValidateClientProfile(registration.Profile)
            : null;
    }

    private static string? ValidateClientProfile(ClientProfileInputDto? profile)
    {
        if (profile is null)
        {
            return "Для роли «Клиент» заполните анкету.";
        }

        if (string.IsNullOrWhiteSpace(profile.PassportNumber))
        {
            return "Укажите номер и серию паспорта клиента.";
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - profile.BirthDate.Year;

        if (age < 0)
        {
            age++;
        }

        if (profile.BirthDate >= today || age < BankConstants.AgeMinimumYears || age > BankConstants.AgeMaximumYears)
        {
            return $"Возраст клиента должен быть от {BankConstants.AgeMinimumYears} " +
                   $"до {BankConstants.AgeMaximumYears} лет.";
        }

        if (string.IsNullOrWhiteSpace(profile.RegistrationAddress))
        {
            return "Укажите адрес регистрации клиента.";
        }

        if (string.IsNullOrWhiteSpace(profile.EmployerName))
        {
            return "Укажите место работы клиента.";
        }

        if (profile.EmploymentMonths < 0)
        {
            return "Стаж работы не может быть отрицательным.";
        }

        if (profile.MonthlyIncome <= 0m)
        {
            return "Укажите ежемесячный доход клиента.";
        }

        return profile.MonthlyExpenses >= 0m ? null : "Ежемесячные расходы не могут быть отрицательными.";
    }

    private static string? ValidateUserEditor(UserEditorDto editor, int administratorUserId)
    {
        if (string.IsNullOrWhiteSpace(editor.FullName))
        {
            return "Укажите фамилию, имя и отчество пользователя.";
        }

        if (editor.UserId == administratorUserId && !editor.IsActive)
        {
            return SelfDeactivationMessage;
        }

        if (editor.UserId == administratorUserId && editor.Role != UserRole.Administrator)
        {
            return SelfRoleChangeMessage;
        }

        if (!string.IsNullOrWhiteSpace(editor.NewPassword) && editor.NewPassword.Length < BankConstants.PasswordMinimumLength)
        {
            return $"Пароль должен содержать не менее {BankConstants.PasswordMinimumLength} символов.";
        }

        return editor.Role == UserRole.Client
            ? ValidateClientProfile(editor.Profile)
            : null;
    }

    private static string? ValidateProduct(ProductEditorDto editor)
    {
        if (string.IsNullOrWhiteSpace(editor.Name))
        {
            return "Укажите наименование кредитного продукта.";
        }

        if (editor.AnnualInterestRate <= 0m)
        {
            return "Процентная ставка должна быть больше нуля.";
        }

        if (editor.AnnualInterestRate > BankConstants.InterestRateMaximumPercent)
        {
            return $"Процентная ставка не может превышать {BankConstants.InterestRateMaximumPercent:0.##}% годовых.";
        }

        if (editor.MinAmount <= 0m || editor.MaxAmount < editor.MinAmount)
        {
            return "Проверьте лимиты суммы кредита: максимальная сумма должна быть не меньше минимальной.";
        }

        if (editor.MinTermMonths <= 0 || editor.MaxTermMonths < editor.MinTermMonths)
        {
            return "Проверьте лимиты срока кредита: максимальный срок должен быть не меньше минимального.";
        }

        return null;
    }
}
