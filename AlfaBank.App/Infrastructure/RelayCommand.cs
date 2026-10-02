using System.Windows.Input;

namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Собственная реализация команды MVVM, не требующая сторонних библиотек.
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>
    /// Создаёт команду с параметром.
    /// </summary>
    /// <param name="execute">Действие, выполняемое при вызове команды.</param>
    /// <param name="canExecute">Условие доступности команды. Не заполняется, если команда всегда доступна.</param>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Создаёт команду без параметра.
    /// </summary>
    /// <param name="execute">Действие, выполняемое при вызове команды.</param>
    /// <param name="canExecute">Условие доступности команды. Не заполняется, если команда всегда доступна.</param>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute is null ? null : _ => canExecute())
    {
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <inheritdoc />
    public void Execute(object? parameter) => _execute(parameter);

    /// <summary>
    /// Оповещает интерфейс о том, что условие доступности команды изменилось.
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
