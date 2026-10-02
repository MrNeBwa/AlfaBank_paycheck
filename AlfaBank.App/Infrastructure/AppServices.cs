using AlfaBank.Core.Data;
using AlfaBank.Core.Security;
using AlfaBank.Core.Services;
using AlfaBank.Core.Services.Contracts;

namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Состав сервисов приложения. Заменяет контейнер внедрения зависимостей:
/// зависимости создаются явно и передаются моделям представления через конструктор.
/// </summary>
public sealed class AppServices
{
    /// <summary>
    /// Создаёт сервисы предметной области и инфраструктуры приложения.
    /// </summary>
    public AppServices()
    {
        IPasswordHasher passwordHasher = new Pbkdf2PasswordHasher();
        ICreditCalculator creditCalculator = new CreditCalculator();
        IScoringService scoringService = new ScoringService();

        UserSessionHolder = new UserSessionHolder();
        Navigation = new NavigationService();

        AuthService = new AuthService(AppDbContextFactory.Create, passwordHasher);
        ClientService = new ClientService(AppDbContextFactory.Create, creditCalculator);
        CreditSpecialistService = new CreditSpecialistService(
            AppDbContextFactory.Create,
            creditCalculator,
            scoringService);
        AdminService = new AdminService(AppDbContextFactory.Create, passwordHasher);

        CreditCalculator = creditCalculator;
    }

    /// <summary>
    /// Данные текущего пользователя.
    /// </summary>
    public UserSessionHolder UserSessionHolder { get; }

    /// <summary>
    /// Служба навигации между страницами.
    /// </summary>
    public NavigationService Navigation { get; }

    /// <summary>
    /// Сервис авторизации и регистрации.
    /// </summary>
    public IAuthService AuthService { get; }

    /// <summary>
    /// Сервис операций клиента.
    /// </summary>
    public IClientService ClientService { get; }

    /// <summary>
    /// Сервис операций кредитного специалиста.
    /// </summary>
    public ICreditSpecialistService CreditSpecialistService { get; }

    /// <summary>
    /// Кредитный калькулятор аннуитетных платежей.
    /// </summary>
    public ICreditCalculator CreditCalculator { get; }

    /// <summary>
    /// Сервис операций администратора.
    /// </summary>
    public IAdminService AdminService { get; }
}
