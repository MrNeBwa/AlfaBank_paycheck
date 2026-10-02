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
/// Сервис администратора: управление учётными записями и справочником кредитных продуктов.
/// </summary>
public sealed class AdminService : IAdminService
{
    private const string LoginAlreadyUsedMessage = "Пользователь с таким логином уже зарегистрирован.";
    private const string UserNotFoundMessage = "Пользователь не найден.";
    private const string ProductNotFoundMessage = "Кредитный продукт не найден.";
    private const string SelfDeactivationMessage = "Нельзя отключить собственную учётную запись.";
    private const string SelfRoleChangeMessage = "Нельзя изменить собственную роль.";

    private readonly Func<AppDbContext> _contextFactory;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Создаёт сервис администратора.
    /// </summary>
    /// <param name="contextFactory">Фабрика контекстов базы данных.</param>
    /// <param name="passwordHasher">Сервис хэширования паролей.</param>
    public AdminService(Func<AppDbContext> contextFactory, IPasswordHasher passwordHasher)
    {
        _contextFactory = contextFactory;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory();

        var users = await context.Users
            .Include(user => user.ClientProfile)
            .OrderBy(user => user.FullName)
            .ToListAsync(cancellationToken);

        return users.Select(EntityMapper.ToUserDto).ToList();
    }

    /// <inheritdoc />
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

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Success(user.Id);
    }

    /// <inheritdoc />
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

        var user = await context.Users.FirstOrDefaultAsync(candidate => candidate.Id == editor.UserId, cancellationToken);

        if (user is null)
        {
            return OperationResult.Failure(UserNotFoundMessage);
        }

        ApplyUserChanges(user, editor);

        await context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <summary>
    /// Загружает продукт, который необходимо изменить.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="productId">Идентификатор продукта. Ноль означает создание нового продукта.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Продукт для изменения либо null.</returns>
    private static Task<CreditProduct?> GetProductToEditAsync(
        AppDbContext context,
        int productId,
        CancellationToken cancellationToken) =>
        context.CreditProducts.FirstOrDefaultAsync(product => product.Id == productId, cancellationToken);

    /// <summary>
    /// Применяет изменения к учётной записи.
    /// </summary>
    /// <param name="user">Изменяемая учётная запись.</param>
    /// <param name="editor">Новые данные учётной записи.</param>
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

    /// <summary>
    /// Применяет изменения к кредитному продукту.
    /// </summary>
    /// <param name="product">Изменяемый продукт.</param>
    /// <param name="editor">Новые данные продукта.</param>
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

    /// <summary>
    /// Проверяет данные регистрации сотрудника.
    /// </summary>
    /// <param name="registration">Данные сотрудника.</param>
    /// <returns>Текст ошибки или null, если данные корректны.</returns>
    private static string? ValidateStaffRegistration(StaffRegistrationDto registration)
    {
        if (string.IsNullOrWhiteSpace(registration.Login))
        {
            return "Введите логин сотрудника.";
        }

        if (string.IsNullOrWhiteSpace(registration.Password) || registration.Password.Length < BankConstants.PasswordMinimumLength)
        {
            return $"Пароль должен содержать не менее {BankConstants.PasswordMinimumLength} символов.";
        }

        if (string.IsNullOrWhiteSpace(registration.FullName))
        {
            return "Укажите фамилию, имя и отчество сотрудника.";
        }

        return registration.Role is UserRole.CreditSpecialist or UserRole.Administrator
            ? null
            : "Учётные записи клиентов создаются клиентами самостоятельно при регистрации.";
    }

    /// <summary>
    /// Проверяет данные изменения учётной записи.
    /// </summary>
    /// <param name="editor">Новые данные учётной записи.</param>
    /// <param name="administratorUserId">Идентификатор администратора.</param>
    /// <returns>Текст ошибки или null, если данные корректны.</returns>
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

        return null;
    }

    /// <summary>
    /// Проверяет данные кредитного продукта.
    /// </summary>
    /// <param name="editor">Данные продукта.</param>
    /// <returns>Текст ошибки или null, если данные корректны.</returns>
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
