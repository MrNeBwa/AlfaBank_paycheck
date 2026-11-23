using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AlfaBank.App.Infrastructure;

public static class PasswordBoxBinding
{
    public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached(
        "Password",
        typeof(string),
        typeof(PasswordBoxBinding),
        new FrameworkPropertyMetadata(string.Empty, OnPasswordChanged)
        {
            BindsTwoWayByDefault = true
        });

    public static readonly DependencyProperty MonitorChangesProperty = DependencyProperty.RegisterAttached(
        "MonitorChanges",
        typeof(bool),
        typeof(PasswordBoxBinding),
        new PropertyMetadata(false, OnMonitorChangesChanged));

    public static string GetPassword(DependencyObject element) => (string)element.GetValue(PasswordProperty);

    public static void SetPassword(DependencyObject element, string value) => element.SetValue(PasswordProperty, value);

    public static bool GetMonitorChanges(DependencyObject element) => (bool)element.GetValue(MonitorChangesProperty);

    public static void SetMonitorChanges(DependencyObject element, bool value) => element.SetValue(MonitorChangesProperty, value);

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