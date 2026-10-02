using System.ComponentModel;
using System.Windows;
using AlfaBank.App.ViewModels;

namespace AlfaBank.App.Views;

/// <summary>
/// Окно входа в систему.
/// </summary>
public partial class LoginWindow : Window
{
    /// <summary>
    /// Создаёт окно входа в систему.
    /// </summary>
    public LoginWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Фабрика окна регистрации клиента. Заполняется точкой входа приложения.
    /// </summary>
    public Func<Window>? RegistrationWindowFactory { get; set; }

    /// <summary>
    /// Открывает окно самостоятельной регистрации клиента.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные события нажатия.</param>
    private void OnRegisterClientClick(object sender, RoutedEventArgs e)
    {
        if (RegistrationWindowFactory is null)
        {
            return;
        }

        var registrationWindow = RegistrationWindowFactory();
        registrationWindow.Owner = this;
        registrationWindow.ShowDialog();
    }

    /// <summary>
    /// Подписывает окно на уведомления модели представления при смене контекста данных.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные изменения контекста данных.</param>
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is LoginViewModel previousViewModel)
        {
            previousViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is LoginViewModel currentViewModel)
        {
            currentViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// Закрывает окно после успешной авторизации пользователя.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные изменения свойства модели представления.</param>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is LoginViewModel viewModel
            && e.PropertyName == nameof(LoginViewModel.IsSignedIn)
            && viewModel.IsSignedIn)
        {
            DialogResult = true;
        }
    }
}