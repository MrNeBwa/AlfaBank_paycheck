using System.ComponentModel;
using System.Windows;
using AlfaBank.App.ViewModels;

namespace AlfaBank.App.Views;

/// <summary>
/// Главное окно приложения с меню ролей и областью содержимого.
/// </summary>
public partial class ShellWindow : Window
{
    /// <summary>
    /// Создаёт главное окно приложения.
    /// </summary>
    public ShellWindow()
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
        if (e.OldValue is ShellViewModel previousViewModel)
        {
            previousViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is ShellViewModel currentViewModel)
        {
            currentViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// Закрывает окно после завершения сеанса пользователя, чтобы приложение
    /// вернуло его к окну входа.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Данные изменения свойства модели представления.</param>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ShellViewModel viewModel
            && e.PropertyName == nameof(ShellViewModel.IsSignedOut)
            && viewModel.IsSignedOut)
        {
            Close();
        }
    }
}
