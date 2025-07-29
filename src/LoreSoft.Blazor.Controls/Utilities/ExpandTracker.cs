namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Tracks the expanded state of items in a component.
/// Provides methods to toggle, clear, and query expansion, and notifies listeners when the state changes.
/// </summary>
/// <typeparam name="TItem">The type of item being tracked for expansion.</typeparam>
public class ExpandTracker<TItem>
{
    private readonly HashSet<TItem> _expandedItems = [];

    /// <summary>
    /// Occurs when the expanded state changes.
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Determines whether the specified item is currently expanded.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns><c>true</c> if the item is expanded; otherwise, <c>false</c>.</returns>
    public bool IsExpanded(TItem item)
        => _expandedItems.Contains(item);

    /// <summary>
    /// Toggles the expanded state of the specified item.
    /// If the item is expanded, it will be collapsed; if collapsed, it will be expanded.
    /// </summary>
    /// <param name="item">The item to toggle.</param>
    public void Toggle(TItem item)
    {
        if (!_expandedItems.Remove(item))
            _expandedItems.Add(item);

        NotifyStateChanged();
    }

    /// <summary>
    /// Clears the expanded state of all items.
    /// </summary>
    public virtual void Clear()
    {
        _expandedItems.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Notifies listeners that the expanded state has changed by invoking <see cref="OnChange"/>.
    /// </summary>
    public void NotifyStateChanged()
        => OnChange?.Invoke();
}
