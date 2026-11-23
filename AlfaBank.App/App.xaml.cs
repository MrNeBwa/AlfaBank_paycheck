using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using AlfaBank.Core.Data;
using AlfaBank.App.Infrastructure;
using AlfaBank.App.ViewModels;
using AlfaBank.App.Views;

namespace AlfaBank.App;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ApplyRussianCulture();
        await RunAsync();
    }

    private async Task RunAsync()
    {
        var initializationResult = await DbInitializer.InitializeAsync();

        if (!initializationResult.IsSuccess)
        {
            MessageBox.Show(initializationResult.ErrorMessage, "Альфабанк", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        var services = new AppServices();

        while (true)
        {
            var loginWindow = new LoginWindow
            {
                DataContext = new LoginViewModel(services.AuthService, services.UserSessionHolder),
                RegistrationWindowFactory = () => new RegisterClientWindow
                {
                    DataContext = new RegisterClientViewModel(services.AuthService, services.UserSessionHolder)
                }
            };

            loginWindow.ShowDialog();

            if (services.UserSessionHolder.CurrentUser is null)
            {
                break;
            }

            var shellWindow = new ShellWindow
            {
                DataContext = new ShellViewModel(services)
            };

            shellWindow.ShowDialog();
            services.UserSessionHolder.SignOut();
            services.Navigation.Reset();
        }

        Shutdown();
    }

    private static void ApplyRussianCulture()
    {
        var russianCulture = new CultureInfo("ru-RU");

        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(russianCulture.IetfLanguageTag)));

        CultureInfo.DefaultThreadCurrentCulture = russianCulture;
        CultureInfo.DefaultThreadCurrentUICulture = russianCulture;
    }
}