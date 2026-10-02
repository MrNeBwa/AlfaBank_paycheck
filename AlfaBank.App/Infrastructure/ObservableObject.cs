using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Базовый класс моделей представления: реализует уведомление об изменении свойств.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Присваивает значение полю и уведомляет об изменении свойства, если значение изменилось.
    /// </summary>
    /// <typeparam name="TValue">Тип значения свойства.</typeparam>
    /// <param name="field">Поле, хранящее текущее значение.</param>
    /// <param name="value">Новое значение.</param>
    /// <param name="propertyName">Имя свойства. Заполняется автоматически.</param>
    /// <returns>Значение, равное true, если значение было изменено.</returns>
    protected bool SetProperty<TValue>(ref TValue field, TValue value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TValue>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);

        return true;
    }

    /// <summary>
    /// Уведомляет подписчиков об изменении свойства.
    /// </summary>
    /// <param name="propertyName">Имя свойства. Заполняется автоматически.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
