namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Методы расширения для задач, выполняемых без ожидания результата.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Запускает задачу без ожидания и предотвращает необработанное исключение.
    /// </summary>
    /// <param name="task">Выполняемая задача.</param>
    public static void Forget(this Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        task.ContinueWith(
            completed => _ = completed.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }
}