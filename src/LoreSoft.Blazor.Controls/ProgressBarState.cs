namespace LoreSoft.Blazor.Controls;

public class ProgressBarState
{
    public event Action OnChange;

    public int Count { get; private set; }

    public bool Loading => Count > 0;

    public void Start()
    {
        Count++;
        NotifyStateChanged();
    }

    public void Complete()
    {
        Count--;
        NotifyStateChanged();
    }

    public void Reset()
    {
        Count = 0;
        NotifyStateChanged();
    }


    protected void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}
