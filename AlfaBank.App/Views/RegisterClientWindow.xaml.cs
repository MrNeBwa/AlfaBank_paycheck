using System.ComponentModel;
using System.Windows;
using AlfaBank.App.ViewModels;

namespace AlfaBank.App.Views;

/// <summary>
/// Окно самостоятельной регистрации клиента.
/// </summary>
public partial class RegisterClientWindow : Window
{
    /// <summary>
    /// Создаёт окно регистрации клиента.
    /// </summary>
    public RegisterClientWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>
    /// Подписывает окно на уведомления модели представления при смене контекста данных.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные изменения контекста данных.</param>
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is RegisterClientViewModel previousViewModel)
        {
            previousViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is RegisterClientViewModel currentViewModel)
        {
            currentViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// Закрывает окно после успешной регистрации клиента.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные изменения свойства модели представления.</param>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is RegisterClientViewModel viewModel
            && e.PropertyName == nameof(RegisterClientViewModel.IsRegistered)
            && viewModel.IsRegistered)
        {
            DialogResult = true;
        }
    }
}