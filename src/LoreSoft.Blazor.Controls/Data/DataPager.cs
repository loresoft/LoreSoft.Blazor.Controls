using System.ComponentModel;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component that provides paging controls for data navigation.
/// Supports boundary, direction, and page navigation, and customizable appearance.
/// </summary>
public class DataPager : ComponentBase, IDisposable
{
    /// <summary>
    /// Gets or sets the pager state, which tracks the current page, page size, and total items.
    /// </summary>
    [CascadingParameter(Name = "PagerState")]
    protected DataPagerState PagerState { get; set; } = new();

    /// <summary>
    /// Gets or sets additional attributes to be applied to the pager container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the page or page size changes.
    /// </summary>
    [Parameter]
    public EventCallback<PageChangedEventArgs> PagerChanged { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page. Default is 25.
    /// </summary>
    [Parameter]
    public int PageSize { get; set; } = 25;

    /// <summary>
    /// Gets or sets the number of page links to display. Default is 5.
    /// </summary>
    [Parameter]
    public int DisplaySize { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether to show first and last page links. Default is true.
    /// </summary>
    [Parameter]
    public bool ShowBoundary { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show previous and next page links. Default is true.
    /// </summary>
    [Parameter]
    public bool ShowDirection { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show individual page links. Default is true.
    /// </summary>
    [Parameter]
    public bool ShowPage { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show the pager when there are no pages. Default is true.
    /// </summary>
    [Parameter]
    public bool ShowEmpty { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the selected page is centered in the page links. Default is true.
    /// </summary>
    [Parameter]
    public bool CenterSelected { get; set; } = true;

    /// <summary>
    /// Gets or sets the text for the previous page button. Default is "‹".
    /// </summary>
    [Parameter]
    public string PreviousText { get; set; } = "‹";

    /// <summary>
    /// Gets or sets the text for the next page button. Default is "›".
    /// </summary>
    [Parameter]
    public string NextText { get; set; } = "›";

    /// <summary>
    /// Gets or sets the text for the first page button. Default is "«".
    /// </summary>
    [Parameter]
    public string FirstText { get; set; } = "«";

    /// <summary>
    /// Gets or sets the text for the last page button. Default is "»".
    /// </summary>
    [Parameter]
    public string LastText { get; set; } = "»";

    /// <summary>
    /// Gets or sets the CSS class for the pager container. Default is "data-pager".
    /// </summary>
    [Parameter]
    public string PagerClass { get; set; } = "data-pager";

    /// <summary>
    /// Gets or sets the CSS class for each page item. Default is "data-page-item".
    /// </summary>
    [Parameter]
    public string ItemClass { get; set; } = "data-page-item";

    /// <summary>
    /// Gets or sets the CSS class for each page button. Default is "data-page-link".
    /// </summary>
    [Parameter]
    public string ButtonClass { get; set; } = "data-page-link";

    /// <summary>
    /// Gets or sets the CSS class for the currently selected page. Default is "active".
    /// </summary>
    [Parameter]
    public string CurrentClass { get; set; } = "active";

    /// <summary>
    /// Gets or sets the CSS class for disabled page links. Default is "disabled".
    /// </summary>
    [Parameter]
    public string DisabledClass { get; set; } = "disabled";

    /// <summary>
    /// Navigates to the first page.
    /// </summary>
    public void FirstPage()
    {
        if (PagerState.IsFirstPage)
            return;

        GoToPage(1);
    }

    /// <summary>
    /// Navigates to the previous page.
    /// </summary>
    public void PreviousPage()
    {
        if (!PagerState.HasPreviousPage)
            return;

        GoToPage(PagerState.Page - 1);
    }

    /// <summary>
    /// Navigates to the next page.
    /// </summary>
    public void NextPage()
    {
        if (!PagerState.HasNextPage)
            return;

        GoToPage(PagerState.Page + 1);
    }

    /// <summary>
    /// Navigates to the last page.
    /// </summary>
    public void LastPage()
    {
        if (PagerState.IsLastPage)
            return;

        GoToPage(PagerState.PageCount);
    }

    /// <summary>
    /// Navigates to the specified page.
    /// </summary>
    /// <param name="page">The page number to navigate to.</param>
    public void GoToPage(int page)
    {
        page = Math.Min(page, PagerState.PageCount);
        page = Math.Max(page, 1);

        if (page == PagerState.Page)
            return;

        PagerState.Page = page;
    }

    /// <summary>
    /// Releases resources used by the pager.
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

        // copy defaults to state
        if (PagerState.PageSize != PageSize)
            PagerState.Attach(1, PageSize);

        PagerState.PropertyChanged += OnStatePropertyChange;
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (PagerState.PageCount == 0 && !ShowEmpty)
            return;

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "role", "navigation");
        builder.AddMultipleAttributes(2, AdditionalAttributes);

        RenderPagination(builder);

        builder.CloseElement(); // div

    }

    /// <summary>
    /// Renders the pagination controls.
    /// </summary>
    /// <param name="builder">The render tree builder.</param>
    private void RenderPagination(RenderTreeBuilder builder)
    {
        builder.OpenElement(3, "ul");
        builder.AddAttribute(4, "class", PagerClass);

        RenderFirstLink(builder);
        RenderPreviousLink(builder);

        if (ShowPage)
        {
            var (start, end) = GetPageEnds();

            if (start > 1)
                RenderPageLink(builder, start - 1, "...", $"Go to page {start - 1}");

            for (var p = start; p <= end; p++)
                RenderPageLink(builder, p, p.ToString(), $"Go to page {p}", p == PagerState.Page ? CurrentClass : null);

            if (end < PagerState.PageCount)
                RenderPageLink(builder, end + 1, "...", $"Go to page {end + 1}");
        }

        RenderNextLink(builder);
        RenderLastLink(builder);

        builder.CloseElement(); // ul
    }

    /// <summary>
    /// Renders the first page link.
    /// </summary>
    /// <param name="builder">The render tree builder.</param>
    private void RenderFirstLink(RenderTreeBuilder builder)
    {
        if (!ShowBoundary)
            return;

        var page = 1;
        var disabledClass = PagerState.IsFirstPage ? DisabledClass : null;

        RenderPageLink(builder, page, FirstText, "Go to first page", disabledClass);
    }

    /// <summary>
    /// Renders the previous page link.
    /// </summary>
    /// <param name="builder">The render tree builder.</param>
    private void RenderPreviousLink(RenderTreeBuilder builder)
    {
        if (!ShowDirection)
            return;

        var page = PagerState.Page - 1;
        var disabledClass = PagerState.HasPreviousPage ? null : DisabledClass;

        RenderPageLink(builder, page, PreviousText, "Go to previous page", disabledClass);
    }

    /// <summary>
    /// Renders a page link button or span.
    /// </summary>
    /// <param name="builder">The render tree builder.</param>
    /// <param name="page">The page number.</param>
    /// <param name="text">The display text.</param>
    /// <param name="title">The tooltip/title text.</param>
    /// <param name="disabledClass">The CSS class for disabled state.</param>
    private void RenderPageLink(RenderTreeBuilder builder, int page, string text, string title, string? disabledClass = null)
    {
        var enabled = string.IsNullOrEmpty(disabledClass);

        var itemClass = CssBuilder
            .Default(ItemClass)
            .AddClass(disabledClass, !enabled)
            .ToString();

        builder.OpenElement(5, "li");
        builder.AddAttribute(6, "class", itemClass);
        builder.SetKey(new { page, text });

        if (enabled)
        {
            builder.OpenElement(7, "button");
            builder.AddAttribute(8, "class", ButtonClass);
            builder.AddAttribute(9, "type", "button");
            builder.AddAttribute(10, "title", title);
            builder.AddAttribute(11, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => GoToPage(page)));
            builder.AddContent(12, text);
            builder.CloseElement(); // button
        }
        else
        {
            builder.OpenElement(13, "span");
            builder.AddAttribute(14, "class", ButtonClass);
            builder.AddContent(15, text);
            builder.CloseElement(); // span
        }

        builder.CloseElement(); // li
    }

    /// <summary>
    /// Renders the next page link.
    /// </summary>
    /// <param name="builder">The render tree builder.</param>
    private void RenderNextLink(RenderTreeBuilder builder)
    {
        if (!ShowDirection)
            return;

        var page = PagerState.Page + 1;
        var disabledClass = PagerState.HasNextPage ? null : DisabledClass;

        RenderPageLink(builder, page, NextText, "Go to next page", disabledClass);
    }

    /// <summary>
    /// Renders the last page link.
    /// </summary>
    /// <param name="builder">The render tree builder.</param>
    private void RenderLastLink(RenderTreeBuilder builder)
    {
        if (!ShowBoundary)
            return;

        var page = PagerState.PageCount;
        var disabledClass = PagerState.IsLastPage ? DisabledClass : null;

        RenderPageLink(builder, page, LastText, "Go to last page", disabledClass);
    }

    /// <summary>
    /// Calculates the start and end page numbers for the displayed page links.
    /// </summary>
    /// <returns>A tuple containing the start and end page numbers.</returns>
    private (int start, int end) GetPageEnds()
    {
        var start = 1;
        var end = PagerState.PageCount;
        var isMax = DisplaySize > 0 && DisplaySize < PagerState.PageCount;

        if (!isMax)
            return (start, end);

        if (CenterSelected)
        {
            int f = (int)Math.Floor(DisplaySize / 2d);

            start = Math.Max(PagerState.Page - f, 1);
            end = start + DisplaySize - 1;

            if (end <= PagerState.PageCount)
                return (start, end);

            end = PagerState.PageCount;
            start = end - DisplaySize + 1;

            return (start, end);
        }

        int c = (int)Math.Ceiling(PagerState.Page / (double)DisplaySize);

        start = (c - 1) * DisplaySize + 1;
        end = Math.Min(start + DisplaySize - 1, PagerState.PageCount);
        return (start, end);
    }

    /// <summary>
    /// Handles property changes in the pager state and triggers UI updates and events.
    /// </summary>
    /// <param name="sender">The sender object.</param>
    /// <param name="e">The property changed event arguments.</param>
    private void OnStatePropertyChange(object? sender, PropertyChangedEventArgs e)
    {
        InvokeAsync(() =>
        {
            StateHasChanged();

            if (PagerChanged.HasDelegate && (e.PropertyName == nameof(DataPagerState.Page) || e.PropertyName == nameof(DataPagerState.PageSize)))
            {
                PagerChanged.InvokeAsync(new PageChangedEventArgs(PagerState.Page, PageSize));
            }
        });
    }

}

/// <summary>
/// Provides data for the page changed event.
/// </summary>
/// <param name="Page">The new page number.</param>
/// <param name="PageSize">The page size.</param>
public record PageChangedEventArgs(int Page, int PageSize);
