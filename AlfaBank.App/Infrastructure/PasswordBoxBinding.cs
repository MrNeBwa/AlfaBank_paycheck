using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Привязка пароля элемента <see cref="PasswordBox"/> к свойству модели представления.
/// </summary>
public static class PasswordBoxBinding
{
    /// <summary>
    /// Присоединённое свойство, связывающее значение <see cref="PasswordBox.Password"/> с источником данных.
    /// Свойство объявлено двусторонне связанным по умолчанию (FrameworkPropertyMetadata),
    /// иначе привязка {Binding Password} была бы односторонней и введённый пароль
    /// не попадал бы в модель представления.
    /// </summary>
    public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached(
        "Password",
        typeof(string),
        typeof(PasswordBoxBinding),
        new FrameworkPropertyMetadata(string.Empty, OnPasswordChanged)
        {
            BindsTwoWayByDefault = true
        });

    /// <summary>
    /// Присоединённое свойство, указывающее на необходимость отслеживания изменений пароля.
    /// </summary>
    public static readonly DependencyProperty MonitorChangesProperty = DependencyProperty.RegisterAttached(
        "MonitorChanges",
        typeof(bool),
        typeof(PasswordBoxBinding),
        new PropertyMetadata(false, OnMonitorChangesChanged));

    /// <summary>
    /// Возвращает значение присоединённого свойства пароля.
    /// </summary>
    /// <param name="element">Элемент <see cref="PasswordBox"/>.</param>
    /// <returns>Текст пароля.</returns>
    public static string GetPassword(DependencyObject element) => (string)element.GetValue(PasswordProperty);

    /// <summary>
    /// Задаёт значение присоединённого свойства пароля.
    /// </summary>
    /// <param name="element">Элемент <see cref="PasswordBox"/>.</param>
    /// <param name="value">Текст пароля.</param>
    public static void SetPassword(DependencyObject element, string value) => element.SetValue(PasswordProperty, value);

    /// <summary>
    /// Возвращает признак отслеживания изменений пароля.
    /// </summary>
    /// <param name="element">Элемент <see cref="PasswordBox"/>.</param>
    /// <returns>Признак отслеживания изменений.</returns>
    public static bool GetMonitorChanges(DependencyObject element) => (bool)element.GetValue(MonitorChangesProperty);

    /// <summary>
    /// Задаёт признак отслеживания изменений пароля.
    /// </summary>
    /// <param name="element">Элемент <see cref="PasswordBox"/>.</param>
    /// <param name="value">Признак отслеживания изменений.</param>
    public static void SetMonitorChanges(DependencyObject element, bool value) => element.SetValue(MonitorChangesProperty, value);

    /// <summary>
    /// Подписывается на событие изменения пароля при включении отслеживания.
    /// </summary>
    /// <param name="dependencyObject">Изменившийся элемент.</param>
    /// <param name="e">Данные изменения свойства.</param>
    private static void OnMonitorChangesChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox)
        {
            return;
        }

        passwordBox.PasswordChanged -= OnPasswordBoxValueChanged;

        if (e.NewValue is true)
        {
            passwordBox.PasswordChanged += OnPasswordBoxValueChanged;
            passwordBox.Password = GetPassword(passwordBox);
        }
    }

    /// <summary>
    /// Синхронизирует значение элемента и источника данных при изменении пароля.
    /// Вызывается также при обновлении привязки, поэтому значение источника
    /// переносится в элемент только если оно действительно отличается.
    /// </summary>
    /// <param name="dependencyObject">Изменившийся элемент.</param>
    /// <param name="e">Данные изменения свойства.</param>
    private static void OnPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox)
        {
            return;
        }

        var password = e.NewValue as string ?? string.Empty;

        if (passwordBox.Password != password)
        {
            passwordBox.Password = password;
        }
    }

    /// <summary>
    /// Передаёт введённый пароль в источник данных при изменении значения элемента.
    /// </summary>
    /// <param name="sender">Элемент <see cref="PasswordBox"/>.</param>
    /// <param name="e">Данные изменения пароля.</param>
    private static void OnPasswordBoxValueChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
        {
            return;
        }

        passwordBox.SetCurrentValue(PasswordProperty, passwordBox.Password);

        BindingOperations.GetBindingExpression(passwordBox, PasswordProperty)?.UpdateSource();
    }
}