namespace AlfaBank.App.Infrastructure;

public sealed class NavigationService : ObservableObject
{
    private object? _currentPage;

    public object? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public void Navigate(object page)
    {
        ArgumentNullException.ThrowIfNull(page);

        CurrentPage = page;

        if (page is IPageViewModel pageViewModel)
        {
            pageViewModel.LoadAsync().Forget();
        }
    }

    public void Reset() => CurrentPage = null;
}
