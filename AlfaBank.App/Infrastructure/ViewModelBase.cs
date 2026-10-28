namespace AlfaBank.App.Infrastructure;

public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string _statusMessage = string.Empty;
    private StatusMessageKind _statusKind = StatusMessageKind.Info;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                // новый текст всегда начинается с нейтрального цвета, иначе старая ошибка «залипает»
                StatusKind = StatusMessageKind.Info;
            }
        }
    }

    public StatusMessageKind StatusKind
    {
        get => _statusKind;
        private set => SetProperty(ref _statusKind, value);
    }

    protected void ShowError(string message)
    {
        StatusMessage = message;
        StatusKind = StatusMessageKind.Error;
    }

    protected void ShowSuccess(string message)
    {
        StatusMessage = message;
        StatusKind = StatusMessageKind.Success;
    }

    protected void ResetStatusMessage()
    {
        StatusMessage = string.Empty;
        StatusKind = StatusMessageKind.Info;
    }

    protected void ReportUnexpectedError(Exception exception) =>
        ShowError($"Непредвиденная ошибка при обращении к базе данных: {exception.Message}");

    protected async Task<TResult?> ExecuteGuardedAsync<TResult>(Func<Task<TResult>> operation)
    {
        IsBusy = true;

        try
        {
            return await operation();
        }
        catch (Exception exception)
        {
            ReportUnexpectedError(exception);
            return default;
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected async Task ExecuteGuardedAsync(Func<Task> operation)
    {
        IsBusy = true;

        try
        {
            await operation();
        }
        catch (Exception exception)
        {
            ReportUnexpectedError(exception);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
