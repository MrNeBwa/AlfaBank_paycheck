using System.ComponentModel;
using System.Windows;
using AlfaBank.App.ViewModels;

namespace AlfaBank.App.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    public Func<Window>? RegistrationWindowFactory { get; set; }

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