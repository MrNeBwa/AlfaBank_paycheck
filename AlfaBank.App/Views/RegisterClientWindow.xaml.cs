using System.ComponentModel;
using System.Windows;
using AlfaBank.App.ViewModels;

namespace AlfaBank.App.Views;

public partial class RegisterClientWindow : Window
{
    public RegisterClientWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

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