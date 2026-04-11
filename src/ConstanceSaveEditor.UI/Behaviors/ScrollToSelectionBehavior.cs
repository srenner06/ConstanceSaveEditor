using Avalonia;
using Avalonia.Controls;

namespace ConstanceSaveEditor.UI.Behaviors;

/// <summary>
/// Attached behavior that scrolls a <see cref="TextBox"/> to its current selection
/// when the bound value changes. Bind <c>Anchor</c> to the same source as
/// <c>SelectionStart</c>; on each change the caret is repositioned, which triggers
/// the TextBox's internal <c>BringIntoView</c> scroll path.
/// </summary>
public static class ScrollToSelectionBehavior
{
    public static readonly AttachedProperty<int> AnchorProperty =
        AvaloniaProperty.RegisterAttached<TextBox, int>(
            "Anchor",
            typeof(ScrollToSelectionBehavior));

    static ScrollToSelectionBehavior()
    {
        AnchorProperty.Changed.AddClassHandler<TextBox>(OnAnchorChanged);
    }

    private static void OnAnchorChanged(TextBox box, AvaloniaPropertyChangedEventArgs e)
    {
        box.CaretIndex = e.GetNewValue<int>();
    }

    public static int GetAnchor(TextBox box) => box.GetValue(AnchorProperty);
    public static void SetAnchor(TextBox box, int value) => box.SetValue(AnchorProperty, value);
}