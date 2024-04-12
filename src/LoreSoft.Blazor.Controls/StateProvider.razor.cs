using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public partial class StateProvider<TState>
{
    [Parameter]
    public required RenderFragment ChildContent { get; set; }

    public event Action<object> OnStateChange;

    public TState? State { get; protected set; }

    public virtual void Set(TState model)
    {
        State = model;
        NotifyStateChanged(this);
    }

    public void NotifyStateChanged(object sender = null)
    {
        StateHasChanged();
        OnStateChange?.Invoke(sender);
    }
}