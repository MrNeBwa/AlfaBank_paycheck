using System.ComponentModel;
using System.Windows;
using AlfaBank.App.ViewModels;

namespace AlfaBank.App.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

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
