namespace AlfaBank.App.Infrastructure;

public static class TaskExtensions
{
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