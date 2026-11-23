using AlfaBank.Core.Data;
using AlfaBank.Core.Security;
using AlfaBank.Core.Services;
using AlfaBank.Core.Services.Contracts;

namespace AlfaBank.App.Infrastructure;

public sealed class AppServices
{
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

    public UserSessionHolder UserSessionHolder { get; }

    public NavigationService Navigation { get; }

    public IAuthService AuthService { get; }

    public IClientService ClientService { get; }

    public ICreditSpecialistService CreditSpecialistService { get; }

    public ICreditCalculator CreditCalculator { get; }

    public IAdminService AdminService { get; }
}
