using System.Collections.ObjectModel;
using AlfaBank.Core.Models.Enums;
using AlfaBank.App.Infrastructure;
using AlfaBank.App.ViewModels.Admin;
using AlfaBank.App.ViewModels.Client;
using AlfaBank.App.ViewModels.Specialist;

namespace AlfaBank.App.ViewModels;

public sealed class ShellViewModel : ViewModelBase
{
    public const string ApplicationTitle = "Альфабанк — автоматизация кредитных операций";

    private readonly UserSessionHolder _userSessionHolder;
    private readonly NavigationService _navigationService;

    private NavigationItem? _selectedItem;
    private bool _isSignedOut;

    public ShellViewModel(AppServices services)
    {
        _userSessionHolder = services.UserSessionHolder;
        _navigationService = services.Navigation;

        Items = BuildMenu(services);
        UserFullName = _userSessionHolder.CurrentUser?.FullName ?? string.Empty;
        UserRole = _userSessionHolder.CurrentUser?.RoleText ?? string.Empty;

        LogOutCommand = new RelayCommand(LogOut);
        SelectedItem = Items[0];
    }

    public string UserFullName { get; }

    public string UserRole { get; }

    public string Title => ApplicationTitle;

    public ObservableCollection<NavigationItem> Items { get; }

    public NavigationItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (!SetProperty(ref _selectedItem, value) || value is null)
            {
                return;
            }

            _navigationService.Navigate(value.Page);
        }
    }

    public RelayCommand LogOutCommand { get; }

    public NavigationService Navigation => _navigationService;

    private static ObservableCollection<NavigationItem> BuildMenu(AppServices services)
    {
        var session = services.UserSessionHolder.CurrentUser;
        var role = session?.Role ?? AlfaBank.Core.Models.Enums.UserRole.Client;

        return role switch
        {
            AlfaBank.Core.Models.Enums.UserRole.CreditSpecialist => BuildSpecialistMenu(services),
            AlfaBank.Core.Models.Enums.UserRole.Administrator => BuildAdministratorMenu(services),
            _ => BuildClientMenu(services)
        };
    }

    private static ObservableCollection<NavigationItem> BuildClientMenu(AppServices services)
    {
        var overview = new ClientOverviewViewModel(services.ClientService, services.UserSessionHolder);

        return
        [
            new NavigationItem("Мои заявки", overview),
            new NavigationItem("Новая заявка", new NewApplicationViewModel(services.ClientService, services.CreditCalculator, services.UserSessionHolder, overview)),
            new NavigationItem("Платежи и график", new ClientScheduleViewModel(services.ClientService, services.UserSessionHolder))
        ];
    }

    private static ObservableCollection<NavigationItem> BuildSpecialistMenu(AppServices services) =>
    [
        new NavigationItem("Очередь заявок", new ApplicationQueueViewModel(services.CreditSpecialistService, services.UserSessionHolder, services.Navigation)),
        new NavigationItem("Кредиты и платежи", new CreditPortfolioViewModel(services.CreditSpecialistService, services.UserSessionHolder))
    ];

    private static ObservableCollection<NavigationItem> BuildAdministratorMenu(AppServices services) =>
    [
        new NavigationItem("Пользователи", new UserManagementViewModel(services.AdminService, services.UserSessionHolder)),
        new NavigationItem("Кредитные продукты", new ProductManagementViewModel(services.AdminService))
    ];

    public bool IsSignedOut
    {
        get => _isSignedOut;
        private set => SetProperty(ref _isSignedOut, value);
    }

    private void LogOut()
    {
        _navigationService.Reset();
        _userSessionHolder.SignOut();

        IsSignedOut = true;
    }
}
