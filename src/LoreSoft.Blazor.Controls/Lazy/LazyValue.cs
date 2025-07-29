using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component that asynchronously loads a value using a key and displays it using a template.
/// </summary>
/// <typeparam name="TKey">The type of the key used to load the value.</typeparam>
/// <typeparam name="TValue">The type of the value to be loaded and displayed.</typeparam>
public class LazyValue<TKey, TValue> : ComponentBase
{
    /// <summary>
    /// Gets or sets the asynchronous method used to load the value based on the provided key.
    /// </summary>
    [Parameter, EditorRequired]
    public required Func<TKey?, Task<TValue?>> LoadMethod { get; set; }

    /// <summary>
    /// Gets or sets the key used to load the value.
    /// </summary>
    [Parameter, EditorRequired]
    public required TKey? Key { get; set; }

    /// <summary>
    /// Gets or sets the template used to render the loaded value.
    /// If not set, the value will be rendered directly using <c>Value.ToString()</c>.
    /// </summary>
    [Parameter]
    public RenderFragment<TValue?>? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the loaded value.
    /// </summary>
    public TValue? Value { get; set; }

    /// <summary>
    /// Called when component parameters are set. Loads the value asynchronously using <see cref="LoadMethod"/> and <see cref="Key"/>.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // load the value async
        Value = await LoadMethod(Key);
    }

    /// <summary>
    /// Builds the render tree for the component, rendering the value using the template if provided.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the component's render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent != null)
            builder.AddContent(0, ChildContent, Value);
        else
            builder.AddContent(1, Value);
    }
}
