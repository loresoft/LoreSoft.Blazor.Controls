using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class LazyValue<TKey, TValue> : ComponentBase
{
    [Parameter, EditorRequired]
    public required Func<TKey?, Task<TValue?>> LoadMethod { get; set; }

    [Parameter, EditorRequired]
    public required TKey? Key { get; set; }

    public TValue? Value { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // load the value async
        Value = await LoadMethod(Key);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddContent(0, Value);
    }
}
