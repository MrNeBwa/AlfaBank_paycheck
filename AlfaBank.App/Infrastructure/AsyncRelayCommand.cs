using System.Windows.Input;

namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Собственная реализация асинхронной команды MVVM.
/// Команда блокируется на время выполнения, чтобы действие не запускалось дважды подряд.
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private readonly Action<Exception>? _onError;

    private bool _isExecuting;

    /// <summary>
    /// Создаёт асинхронную команду с параметром.
    /// </summary>
    /// <param name="execute">Асинхронное действие команды.</param>
    /// <param name="canExecute">Условие доступности команды.</param>
    /// <param name="onError">Обработчик непредвиденной ошибки.</param>
    public AsyncRelayCommand(
        Func<object?, Task> execute,
        Func<object?, bool>? canExecute = null,
        Action<Exception>? onError = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _onError = onError;
    }

    /// <summary>
    /// Создаёт асинхронную команду без параметра.
    /// </summary>
    /// <param name="execute">Асинхронное действие команды.</param>
    /// <param name="canExecute">Условие доступности команды.</param>
    /// <param name="onError">Обработчик непредвиденной ошибки.</param>
    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null, Action<Exception>? onError = null)
        : this(_ => execute(), canExecute is null ? null : _ => canExecute(), onError)
    {
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Признак выполнения команды в текущий момент.
    /// </summary>
    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            _isExecuting = value;
            RaiseCanExecuteChanged();
        }
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

    /// <inheritdoc />
    public async void Execute(object? parameter) => await ExecuteAsync(parameter);

    /// <summary>
    /// Выполняет команду и возвращает задачу. Используется в моделях представления.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    /// <returns>Задача выполнения команды.</returns>
    public async Task ExecuteAsync(object? parameter = null)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        IsExecuting = true;

        try
        {
            await _execute(parameter);
        }
        catch (Exception exception) when (_onError is not null)
        {
            _onError(exception);
        }
        finally
        {
            IsExecuting = false;
        }
    }

    /// <summary>
    /// Оповещает интерфейс о том, что условие доступности команды изменилось.
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
