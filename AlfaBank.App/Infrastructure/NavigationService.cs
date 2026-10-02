namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Служба навигации между страницами приложения.
/// </summary>
public sealed class NavigationService : ObservableObject
{
    private object? _currentPage;

    /// <summary>
    /// Текущая открытая страница: модель представления, для которой подбирается представление.
    /// </summary>
    public object? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    /// <summary>
    /// Открывает указанную страницу.
    /// </summary>
    /// <param name="page">Модель представления открываемой страницы.</param>
    public void Navigate(object page)
    {
        ArgumentNullException.ThrowIfNull(page);

        CurrentPage = page;
    }

    /// <summary>
    /// Сбрасывает текущую страницу при выходе пользователя из системы.
    /// </summary>
    public void Reset() => CurrentPage = null;
}
