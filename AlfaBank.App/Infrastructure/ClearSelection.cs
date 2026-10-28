using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace AlfaBank.App.Infrastructure;

public static class ClearSelection
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ClearSelection),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

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
