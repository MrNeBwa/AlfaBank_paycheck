using System.Collections.ObjectModel;
using AlfaBank.Core.Models.Enums;
using AlfaBank.App.Infrastructure;
using AlfaBank.App.ViewModels.Admin;
using AlfaBank.App.ViewModels.Client;
using AlfaBank.App.ViewModels.Specialist;

namespace AlfaBank.App.ViewModels;

/// <summary>
/// Модель представления главного окна программы: меню навигации и завершение сеанса.
/// </summary>
public sealed class ShellViewModel : ViewModelBase
{
    /// <summary>
    /// Название банка, отображаемое в заголовке окна.
    /// </summary>
    public const string ApplicationTitle = "Альфабанк — автоматизация кредитных операций";

    private readonly UserSessionHolder _userSessionHolder;
    private readonly NavigationService _navigationService;

    private NavigationItem? _selectedItem;
    private bool _isSignedOut;

    /// <summary>
    /// Создаёт модель представления главного окна и строит меню согласно роли пользователя.
    /// </summary>
    /// <param name="services">Сервисы приложения.</param>
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

    /// <summary>
    /// Полное имя авторизованного пользователя.
    /// </summary>
    public string UserFullName { get; }

    /// <summary>
    /// Роль авторизованного пользователя.
    /// </summary>
    public string UserRole { get; }

    /// <summary>
    /// Название программы.
    /// </summary>
    public string Title => ApplicationTitle;

    /// <summary>
    /// Пункты меню, доступные текущей роли.
    /// </summary>
    public ObservableCollection<NavigationItem> Items { get; }

    /// <summary>
    /// Выбранный пункт меню. Его смена открывает соответствующую страницу.
    /// </summary>
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

    /// <summary>
    /// Команда завершения сеанса пользователя.
    /// </summary>
    public RelayCommand LogOutCommand { get; }

    /// <summary>
    /// Служба навигации, текущая страница которой отображается в области содержимого.
    /// </summary>
    public NavigationService Navigation => _navigationService;

    /// <summary>
    /// Формирует меню навигации в зависимости от роли пользователя.
    /// </summary>
    /// <param name="services">Сервисы приложения.</param>
    /// <returns>Коллекция пунктов меню.</returns>
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

    /// <summary>
    /// Формирует меню клиента.
    /// </summary>
    /// <param name="services">Сервисы приложения.</param>
    /// <returns>Коллекция пунктов меню клиента.</returns>
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

    /// <summary>
    /// Формирует меню кредитного специалиста.
    /// </summary>
    /// <param name="services">Сервисы приложения.</param>
    /// <returns>Коллекция пунктов меню кредитного специалиста.</returns>
    private static ObservableCollection<NavigationItem> BuildSpecialistMenu(AppServices services) =>
    [
        new NavigationItem("Очередь заявок", new ApplicationQueueViewModel(services.CreditSpecialistService, services.UserSessionHolder, services.Navigation)),
        new NavigationItem("Кредиты и платежи", new CreditPortfolioViewModel(services.CreditSpecialistService, services.UserSessionHolder))
    ];

    /// <summary>
    /// Формирует меню администратора.
    /// </summary>
    /// <param name="services">Сервисы приложения.</param>
    /// <returns>Коллекция пунктов меню администратора.</returns>
    private static ObservableCollection<NavigationItem> BuildAdministratorMenu(AppServices services) =>
    [
        new NavigationItem("Пользователи", new UserManagementViewModel(services.AdminService, services.UserSessionHolder)),
        new NavigationItem("Кредитные продукты", new ProductManagementViewModel(services.AdminService))
    ];

    /// <summary>
    /// Признак завершения сеанса. Значение true сигнализирует главному окну о том, что его нужно закрыть.
    /// </summary>
    public bool IsSignedOut
    {
        get => _isSignedOut;
        private set => SetProperty(ref _isSignedOut, value);
    }

    /// <summary>
    /// Завершает сеанс пользователя.
    /// </summary>
    private void LogOut()
    {
        _navigationService.Reset();
        _userSessionHolder.SignOut();

        // Сигнал окну закрыться: без него главное окно оставалось открытым
        // уже без сеанса, и цикл входа в приложение не запускал окно входа заново.
        IsSignedOut = true;
    }
}
