namespace LoreSoft.Blazor.Controls.Utilities;

public class ExpandTracker<TItem>
{
    private readonly HashSet<TItem> _expandedItems = [];

    public event Action OnChange;

    public bool IsExpanded(TItem item)
    {
        return _expandedItems.Contains(item);
    }

    public void Toggle(TItem item)
    {
        if (_expandedItems.Contains(item))
            _expandedItems.Remove(item);
        else
            _expandedItems.Add(item);

        NotifyStateChanged();
    }

    public virtual void Clear()
    {
        _expandedItems.Clear();
        NotifyStateChanged();
    }

    public void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}