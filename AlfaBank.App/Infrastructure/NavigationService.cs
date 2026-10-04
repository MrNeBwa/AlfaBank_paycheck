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
    /// Открывает указанную страницу и загружает её данные.
    /// </summary>
    /// <remarks>
    /// Загрузка выполняется здесь, а не в точке вызова, чтобы страница,
    /// открытая из другой страницы, не осталась пустой: без неё привязки
    /// не наполняются, а кнопки решения остаются заблокированными.
    /// </remarks>
    /// <param name="page">Модель представления открываемой страницы.</param>
    public void Navigate(object page)
    {
        ArgumentNullException.ThrowIfNull(page);

        CurrentPage = page;

        if (page is IPageViewModel pageViewModel)
        {
            pageViewModel.LoadAsync().Forget();
        }
    }

    /// <summary>
    /// Сбрасывает текущую страницу при выходе пользователя из системы.
    /// </summary>
    public void Reset() => CurrentPage = null;
}
