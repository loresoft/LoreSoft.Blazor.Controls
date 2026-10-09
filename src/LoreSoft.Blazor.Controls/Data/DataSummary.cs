using System.ComponentModel;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component that displays a summary of the currently visible item range in a paged data view,
/// for example "1 - 10 of 100". Integrates with <see cref="DataPagerState"/> to update automatically.
/// </summary>
public class DataSummary : ComponentBase, IDisposable
{
    /// <summary>
    /// Gets or sets the pager state, which tracks the current page, page size and total.
    /// </summary>
    [CascadingParameter(Name = "PagerState")]
    protected DataPagerState PagerState { get; set; } = new();

    /// <summary>
    /// Gets or sets additional attributes to be applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the separator displayed between the start and end item.
    /// </summary>
    [Parameter]
    public string RangeSeparator { get; set; } = " - ";

    /// <summary>
    /// Gets or sets the label displayed between the end item and the total.
    /// </summary>
    [Parameter]
    public string TotalLabel { get; set; } = " of ";

    /// <summary>
    /// Gets or sets an optional template to customize the rendered content.
    /// </summary>
    [Parameter]
    public RenderFragment<DataPagerState>? ChildContent { get; set; }

    /// <summary>
    /// Unsubscribes from pager state events and releases resources.
    /// </summary>
    public void Dispose()
    {
        PagerState.PropertyChanged -= OnStatePropertyChange;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (PagerState == null)
            throw new InvalidOperationException("DataSummary requires a cascading parameter PagerState.");

        PagerState.PropertyChanged += OnStatePropertyChange;
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "data-summary");
        builder.AddMultipleAttributes(2, AdditionalAttributes);

        if (ChildContent != null)
        {
            builder.AddContent(3, ChildContent, PagerState);
        }
        else
        {
            builder.AddContent(4, PagerState.StartItem);
            builder.AddContent(5, RangeSeparator);
            builder.AddContent(6, PagerState.EndItem);
            builder.AddContent(7, TotalLabel);
            builder.AddContent(8, PagerState.Total);
        }

        builder.CloseElement(); // div
    }

    private void OnStatePropertyChange(object? sender, PropertyChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }
}
