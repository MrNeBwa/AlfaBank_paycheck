using System.Windows;

namespace AlfaBank.App.Infrastructure;

public interface IPageViewModel
{
    Task LoadAsync();
}