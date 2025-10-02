using System.ComponentModel;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component that allows users to select the number of items displayed per page in a paged data view.
/// Integrates with <see cref="DataPagerState"/> to update page size and supports custom options and labels.
/// </summary>
public class DataSizer : ComponentBase, IDisposable
{
    /// <summary>
    /// Gets or sets the pager state, which tracks the current page and page size.
    /// </summary>
    [CascadingParameter(Name = "PagerState")]
    protected DataPagerState PagerState { get; set; } = new();

    /// <summary>
    /// Gets or sets additional attributes to be applied to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the initial page size to use. If set, overrides the default in <see cref="PagerState"/>.
    /// </summary>
    [Parameter]
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets the list of selectable page size options.
    /// </summary>
    [Parameter]
    public int[] PageSizeOptions { get; set; } = [10, 25, 50, 100];

    /// <summary>
    /// Gets or sets a value indicating whether to include an "All" option (show all items) in the selector.
    /// </summary>
    [Parameter]
    public bool IncludeAllOption { get; set; }

    /// <summary>
    /// Gets or sets the label displayed next to the page size selector.
    /// </summary>
    [Parameter]
    public string DescriptionLabel { get; set; } = "Items per page";

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
            throw new InvalidOperationException("DataSizer requires a cascading parameter PagerState.");

        if (PageSize.HasValue && PagerState.PageSize != PageSize )
            PagerState.Attach(1, PageSize.Value);

        if (!PageSizeOptions.Contains(PagerState.PageSize))
            PageSizeOptions = PageSizeOptions.Append(PagerState.PageSize).Order().ToArray();

        PagerState.PropertyChanged += OnStatePropertyChange;
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "data-sizer");
        builder.AddMultipleAttributes(2, AdditionalAttributes);

        builder.OpenElement(3, "select");
        builder.AddAttribute(4, "name", "page-size");
        builder.AddAttribute(5, "value", PagerState.PageSize);
        builder.AddAttribute(6, "title", "Select page size");
        builder.AddAttribute(7, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => SetPageSize(Convert.ToInt32(e.Value ?? 0))));

        foreach (var pageSizeOption in PageSizeOptions)
        {
            builder.OpenElement(8, "option");
            builder.AddAttribute(9, "value", pageSizeOption);
            builder.SetKey(pageSizeOption);
            builder.AddContent(10, pageSizeOption);
            builder.CloseElement(); // option
        }

        if (IncludeAllOption)
        {
            builder.OpenElement(11, "option");
            builder.AddAttribute(12, "value", 0);
            builder.AddContent(13, "All");
            builder.CloseElement(); // option
        }

        builder.CloseElement(); // select

        if (!string.IsNullOrEmpty(DescriptionLabel))
        {
            builder.OpenElement(14, "span");
            builder.AddContent(15, DescriptionLabel);
            builder.CloseElement(); // span
        }

        builder.CloseElement(); // div
    }

    /// <summary>
    /// Sets the page size in the pager state.
    /// </summary>
    /// <param name="pageSize">The new page size value.</param>
    private void SetPageSize(int pageSize)
    {
        PagerState.PageSize = pageSize;
    }

    /// <summary>
    /// Handles property changes in the pager state and triggers UI updates when the page size changes.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The property changed event arguments.</param>
    private void OnStatePropertyChange(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DataPagerState.PageSize))
            return;

        InvokeAsync(StateHasChanged);
    }

}
