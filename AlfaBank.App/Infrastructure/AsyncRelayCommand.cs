using System.Windows;
using System.Windows.Input;

namespace AlfaBank.App.Infrastructure;

public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private readonly Action<Exception>? _onError;

    private bool _isExecuting;

    public AsyncRelayCommand(
        Func<object?, Task> execute,
        Func<object?, bool>? canExecute = null,
        Action<Exception>? onError = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _onError = onError;
    }

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null, Action<Exception>? onError = null)
        : this(_ => execute(), canExecute is null ? null : _ => canExecute(), onError)
    {
    }

    public event EventHandler? CanExecuteChanged;

    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            _isExecuting = value;
            RaiseCanExecuteChanged();
        }
    }

    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

    public async void Execute(object? parameter) => await ExecuteAsync(parameter);

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

    public void RaiseCanExecuteChanged()
    {
        var handler = CanExecuteChanged;

        if (handler is null)
        {
            return;
        }

        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.BeginInvoke(new Action(() => handler(this, EventArgs.Empty)));
            return;
        }

        handler(this, EventArgs.Empty);
    }
}
