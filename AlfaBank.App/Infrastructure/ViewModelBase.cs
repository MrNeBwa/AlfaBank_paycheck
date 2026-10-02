namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Базовый класс моделей представления: единое состояние занятости и сообщения об ошибках.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Признак выполнения длительной операции. Используется для блокировки элементов интерфейса.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Текст сообщения для пользователя: результат операции либо описание ошибки.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// Формирует сообщение о непредвиденной ошибке и показывает его пользователю.
    /// </summary>
    /// <param name="exception">Исключение, возникшее при выполнении операции.</param>
    protected void ReportUnexpectedError(Exception exception) =>
        StatusMessage = $"Непредвиденная ошибка при обращении к базе данных: {exception.Message}";

    /// <summary>
    /// Выполняет асинхронную операцию, устанавливая признак занятости и обрабатывая ошибки.
    /// </summary>
    /// <typeparam name="TResult">Тип результата операции.</typeparam>
    /// <param name="operation">Операция, требующая доступа к базе данных.</param>
    /// <returns>Результат операции.</returns>
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

    /// <summary>
    /// Выполняет асинхронную операцию без результата, устанавливая признак занятости.
    /// </summary>
    /// <param name="operation">Операция, требующая доступа к базе данных.</param>
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
