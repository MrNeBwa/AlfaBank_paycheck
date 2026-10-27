using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace AlfaBank.App.Infrastructure;

/// <summary>
/// Снимает выделение с элемента списка, если щелчок сделан по пустому месту,
/// а также по нажатию клавиши Escape. Снимает главную проблему интерфейса:
/// выделенная строка остаётся подсвеченной, пока пользователь не щёлкнет по другой.
/// </summary>
public static class ClearSelection
{
    /// <summary>
    /// Включает поведение снятия выделения для указанного списка или таблицы.
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ClearSelection),
            new PropertyMetadata(false, OnIsEnabledChanged));

    /// <summary>
    /// Задаёт признак включения поведения снятия выделения.
    /// </summary>
    /// <param name="element">Список или таблица.</param>
    /// <param name="value">Признак включения.</param>
    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    /// <summary>
    /// Возвращает признак включения поведения снятия выделения.
    /// </summary>
    /// <param name="element">Список или таблица.</param>
    /// <returns>Признак включения.</returns>
    public static bool GetIsEnabled(DependencyObject element) =>
        (bool)element.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
    {
        if (source is not Selector selector)
        {
            return;
        }

        selector.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        selector.PreviewKeyDown -= OnPreviewKeyDown;

        if (args.NewValue is not true)
        {
            return;
        }

        selector.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        selector.PreviewKeyDown += OnPreviewKeyDown;
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
    {
        if (sender is not Selector selector || selector.SelectedItem is null)
        {
            return;
        }

        var source = args.OriginalSource as DependencyObject;

        // Щелчок по строке, ячейке или заголовку столбца не должен снимать выделение:
        // там по-прежнему работает выбор строки и сортировка.
        if (FindAncestor<DataGridCell>(source) is not null
            || FindAncestor<DataGridColumnHeader>(source) is not null
            || FindAncestor<ListBoxItem>(source) is not null
            || FindAncestor<ListBoxItem>(selector) is not null)
        {
            return;
        }

        selector.SelectedItem = null;

        if (selector is DataGrid dataGrid)
        {
            dataGrid.SelectedCells.Clear();
            dataGrid.CurrentCell = default;
        }
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs args)
    {
        if (args.Key != Key.Escape || sender is not Selector selector || selector.SelectedItem is null)
        {
            return;
        }

        selector.SelectedItem = null;

        if (selector is DataGrid dataGrid)
        {
            dataGrid.SelectedCells.Clear();
            dataGrid.CurrentCell = default;
        }

        args.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject? element) where T : DependencyObject
    {
        while (element is not null)
        {
            if (element is T match)
            {
                return match;
            }

            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }
}
