namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Manages the state of a progress bar, including tracking active operations and notifying listeners of state changes.
/// </summary>
public class ProgressBarState
{
    /// <summary>
    /// Occurs when the progress bar state changes.
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Gets the number of active operations being tracked.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the progress bar should be shown (i.e., if any operations are active).
    /// </summary>
    public bool Loading => Count > 0;

    /// <summary>
    /// Increments the count of active operations and notifies listeners.
    /// </summary>
    public void Start()
    {
        Count++;
        NotifyStateChanged();
    }

    /// <summary>
    /// Decrements the count of active operations and notifies listeners.
    /// </summary>
    public void Complete()
    {
        Count--;
        NotifyStateChanged();
    }

    /// <summary>
    /// Resets the count of active operations to zero and notifies listeners.
    /// </summary>
    public void Reset()
    {
        Count = 0;
        NotifyStateChanged();
    }

    /// <summary>
    /// Notifies listeners that the progress bar state has changed.
    /// </summary>
    protected void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}
