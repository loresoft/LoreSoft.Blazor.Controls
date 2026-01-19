using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component that asynchronously loads a value using a key and displays it using a template.
/// </summary>
/// <typeparam name="TKey">The type of the key used to load the value.</typeparam>
/// <typeparam name="TValue">The type of the value to be loaded and displayed.</typeparam>
[CascadingTypeParameter(nameof(TKey))]
[CascadingTypeParameter(nameof(TValue))]
public class LazyValue<TKey, TValue> : ComponentBase
{
    private TKey? _previousKey;

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
    /// Gets or sets the template used to render content while the value is being loaded.
    /// If not set, <see cref="LoadingText"/> or <see cref="Key"/> will be displayed.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// Gets or sets the text to display while the value is being loaded.
    /// Only used if <see cref="LoadingTemplate"/> is not set.
    /// </summary>
    [Parameter]
    public string? LoadingText { get; set; }

    /// <summary>
    /// Gets or sets the loaded value.
    /// </summary>
    public TValue? Value { get; set; }

    /// <summary>
    /// Indicates whether the component is currently loading a value.
    /// </summary>
    public bool Loading { get; set; }

    /// <summary>
    /// Called when component parameters are set. Loads the value asynchronously using <see cref="LoadMethod"/> and <see cref="Key"/>.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // Only load if the key has changed
        if (EqualityComparer<TKey>.Default.Equals(Key, _previousKey))
            return;

        Loading = true;
        try
        {
            // load the value async
            Value = await LoadMethod(Key);
            _previousKey = Key;
        }
        finally
        {
            Loading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Builds the render tree for the component, rendering the value using the template if provided.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the component's render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Loading)
        {
            if (LoadingTemplate != null)
                builder.AddContent(0, LoadingTemplate);
            else if (!string.IsNullOrEmpty(LoadingText))
                builder.AddContent(0, LoadingText);
            else
                builder.AddContent(0, Key);
        }
        else if (ChildContent != null)
        {
            builder.AddContent(0, ChildContent, Value);
        }
        else if (Value == null)
        {
            builder.AddContent(0, Key);
        }
        else
        {
            builder.AddContent(0, Value);
        }
    }
}
