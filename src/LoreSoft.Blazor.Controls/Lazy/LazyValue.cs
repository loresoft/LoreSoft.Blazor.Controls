using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class LazyValue<TKey, TValue> : ComponentBase
{
    [Parameter, EditorRequired]
    public required Func<TKey?, Task<TValue?>> LoadMethod { get; set; }

    [Parameter, EditorRequired]
    public required TKey? Key { get; set; }

    [Parameter]
    public RenderFragment<TValue?>? ChildTemplate { get; set; }

    public TValue? Value { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // load the value async
        Value = await LoadMethod(Key);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildTemplate != null)
            builder.AddContent(0, ChildTemplate, Value);
        else
            builder.AddContent(1, Value);
    }
}
