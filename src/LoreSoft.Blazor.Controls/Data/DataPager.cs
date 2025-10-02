using System.ComponentModel;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A comprehensive pagination component that provides navigation controls for data browsing.
/// This component renders accessible pagination controls with support for boundary navigation (first/last),
/// directional navigation (previous/next), and individual page links. It integrates seamlessly with
/// <see cref="DataPagerState"/> to provide real-time pagination state management and supports
/// extensive customization of appearance, behavior, and accessibility features.
/// </summary>
public class DataPager : ComponentBase, IDisposable
{
    /// <summary>
    /// Gets or sets the pager state, which tracks the current page, page size, and total items.
    /// This cascading parameter is typically provided by parent data components like
    /// <see cref="DataGrid{TItem}"/> or <see cref="DataList{TItem}"/> to coordinate pagination state.
    /// </summary>
    [CascadingParameter(Name = "PagerState")]
    protected DataPagerState PagerState { get; set; } = new();

    /// <summary>
    /// Gets or sets additional attributes to be applied to the pager container.
    /// These attributes are merged with the component's default attributes and applied
    /// to the outermost navigation element, allowing for custom styling and behavior.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the page or page size changes.
    /// This event provides <see cref="PageChangedEventArgs"/> containing the new page number and page size,
    /// allowing parent components to respond to pagination changes for data loading or other operations.
    /// </summary>
    [Parameter]
    public EventCallback<PageChangedEventArgs> PagerChanged { get; set; }

    /// <summary>
    /// Gets or sets the number of items to display per page.
    /// This value is synchronized with the <see cref="DataPagerState.PageSize"/> and is used
    /// to initialize the pager state if not already set. Changes to this value will reset pagination to page 1.
    /// </summary>
    [Parameter]
    public int PageSize { get; set; } = 25;

    /// <summary>
    /// Gets or sets the maximum number of individual page links to display in the pagination controls.
    /// When the total number of pages exceeds this value, the pager will show a subset of pages
    /// with ellipsis (...) indicators for hidden pages. A value of 0 or negative disables this limit.
    /// </summary>
    [Parameter]
    public int DisplaySize { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether to show first and last page navigation buttons.
    /// When true, displays buttons that allow users to jump directly to the first or last page,
    /// providing quick navigation for large datasets. The buttons are automatically disabled
    /// when already on the respective boundary pages.
    /// </summary>
    [Parameter]
    public bool ShowBoundary { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show previous and next page navigation buttons.
    /// When true, displays directional navigation buttons that allow users to move one page
    /// forward or backward. The buttons are automatically disabled when at the beginning or end
    /// of the page range.
    /// </summary>
    [Parameter]
    public bool ShowDirection { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show individual page number links.
    /// When true, displays clickable page numbers allowing direct navigation to specific pages.
    /// The number of visible page links is controlled by <see cref="DisplaySize"/> and <see cref="CenterSelected"/>.
    /// </summary>
    [Parameter]
    public bool ShowPage { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show the pager when there are no pages to display.
    /// When true, the pager is visible even when the total page count is zero, which can be useful
    /// for maintaining consistent layout. When false, the pager is hidden when there are no pages.
    /// </summary>
    [Parameter]
    public bool ShowEmpty { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the currently selected page should be centered
    /// within the displayed page links. When true, the current page appears in the middle of
    /// the visible page range when possible. When false, pages are displayed in sequential blocks.
    /// </summary>
    [Parameter]
    public bool CenterSelected { get; set; } = true;

    /// <summary>
    /// Gets or sets the display text for the previous page navigation button.
    /// This text is used both as the button content and in accessibility labels.
    /// Supports HTML entities and Unicode characters for custom arrow styles.
    /// </summary>
    [Parameter]
    public string PreviousText { get; set; } = "‹";

    /// <summary>
    /// Gets or sets the display text for the next page navigation button.
    /// This text is used both as the button content and in accessibility labels.
    /// Supports HTML entities and Unicode characters for custom arrow styles.
    /// </summary>
    [Parameter]
    public string NextText { get; set; } = "›";

    /// <summary>
    /// Gets or sets the display text for the first page navigation button.
    /// This text is used both as the button content and in accessibility labels.
    /// Supports HTML entities and Unicode characters for custom arrow styles.
    /// </summary>
    [Parameter]
    public string FirstText { get; set; } = "«";

    /// <summary>
    /// Gets or sets the display text for the last page navigation button.
    /// This text is used both as the button content and in accessibility labels.
    /// Supports HTML entities and Unicode characters for custom arrow styles.
    /// </summary>
    [Parameter]
    public string LastText { get; set; } = "»";

    /// <summary>
    /// Gets or sets the CSS class applied to the main pagination container (ul element).
    /// This class defines the overall styling and layout of the pagination component.
    /// Can be used to integrate with CSS frameworks like Bootstrap, Tailwind, or custom designs.
    /// </summary>
    [Parameter]
    public string PagerClass { get; set; } = "data-pager";

    /// <summary>
    /// Gets or sets the CSS class applied to each pagination item container (li elements).
    /// This class is applied to all page items including navigation buttons and page numbers,
    /// providing consistent styling across all pagination elements.
    /// </summary>
    [Parameter]
    public string ItemClass { get; set; } = "data-page-item";

    /// <summary>
    /// Gets or sets the CSS class applied to each clickable pagination button or span element.
    /// This class defines the appearance of individual navigation elements and page numbers,
    /// including hover states, focus indicators, and interactive styling.
    /// </summary>
    [Parameter]
    public string ButtonClass { get; set; } = "data-page-link";

    /// <summary>
    /// Gets or sets the CSS class applied to the currently selected page item.
    /// This class is added to the item container (li element) of the current page,
    /// allowing for distinct visual highlighting of the active page number.
    /// </summary>
    [Parameter]
    public string CurrentClass { get; set; } = "active";

    /// <summary>
    /// Gets or sets the CSS class applied to disabled navigation elements.
    /// This class is added to navigation buttons that cannot be activated (e.g., previous button
    /// on the first page), providing visual feedback about unavailable navigation options.
    /// </summary>
    [Parameter]
    public string DisabledClass { get; set; } = "disabled";

    /// <summary>
    /// Navigates to the first page in the pagination sequence.
    /// This method is safe to call regardless of the current page and will only trigger
    /// navigation if not already on the first page. Updates the pager state and triggers events.
    /// </summary>
    public void FirstPage()
    {
        if (PagerState.IsFirstPage)
            return;

        GoToPage(1);
    }

    /// <summary>
    /// Navigates to the previous page in the pagination sequence.
    /// This method decrements the current page by one if a previous page exists.
    /// Safe to call from any page as it checks for availability before navigation.
    /// </summary>
    public void PreviousPage()
    {
        if (!PagerState.HasPreviousPage)
            return;

        GoToPage(PagerState.Page - 1);
    }

    /// <summary>
    /// Navigates to the next page in the pagination sequence.
    /// This method increments the current page by one if a next page exists.
    /// Safe to call from any page as it checks for availability before navigation.
    /// </summary>
    public void NextPage()
    {
        if (!PagerState.HasNextPage)
            return;

        GoToPage(PagerState.Page + 1);
    }

    /// <summary>
    /// Navigates to the last page in the pagination sequence.
    /// This method jumps directly to the final page based on the total page count.
    /// Safe to call regardless of the current page and will only trigger navigation if not already on the last page.
    /// </summary>
    public void LastPage()
    {
        if (PagerState.IsLastPage)
            return;

        GoToPage(PagerState.PageCount);
    }

    /// <summary>
    /// Navigates to the specified page number with automatic bounds checking.
    /// This method ensures the target page is within valid bounds (1 to PageCount) and
    /// only triggers state changes and events if the page actually changes.
    /// </summary>
    /// <param name="page">The target page number (1-based). Values outside the valid range
    /// are automatically clamped to the nearest valid page.</param>
    public void GoToPage(int page)
    {
        page = Math.Min(page, PagerState.PageCount);
        page = Math.Max(page, 1);

        if (page == PagerState.Page)
            return;

        PagerState.Page = page;
    }

    /// <summary>
    /// Releases resources used by the pagination component.
    /// This method unsubscribes from pager state change events to prevent memory leaks
    /// and ensures proper cleanup when the component is disposed.
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

        builder.OpenElement(0, "nav");
        builder.AddAttribute(1, "role", "navigation");
        builder.AddAttribute(2, "aria-label", "Pagination Navigation");
        builder.AddMultipleAttributes(3, AdditionalAttributes);

        RenderPagination(builder);

        builder.CloseElement(); // nav

    }

    /// <summary>
    /// Renders the main pagination controls container and orchestrates the rendering of all pagination elements.
    /// This method creates the unordered list structure and delegates to specific rendering methods
    /// for each type of pagination control (boundary, directional, and page links).
    /// </summary>
    /// <param name="builder">The render tree builder used to construct the component's HTML structure.</param>
    private void RenderPagination(RenderTreeBuilder builder)
    {
        builder.OpenElement(3, "ul");
        builder.AddAttribute(4, "role", "list");
        builder.AddAttribute(5, "class", PagerClass);

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
    /// Renders the first page navigation link with appropriate styling and accessibility attributes.
    /// This method creates a boundary navigation element that allows users to jump to the first page.
    /// The link is automatically disabled when already on the first page.
    /// </summary>
    /// <param name="builder">The render tree builder used to construct the link element.</param>
    private void RenderFirstLink(RenderTreeBuilder builder)
    {
        if (!ShowBoundary)
            return;

        var page = 1;
        var disabledClass = PagerState.IsFirstPage ? DisabledClass : null;

        RenderPageLink(builder, page, FirstText, "Go to first page", disabledClass);
    }

    /// <summary>
    /// Renders the previous page navigation link with appropriate styling and accessibility attributes.
    /// This method creates a directional navigation element that allows users to move backward one page.
    /// The link is automatically disabled when on the first page.
    /// </summary>
    /// <param name="builder">The render tree builder used to construct the link element.</param>
    private void RenderPreviousLink(RenderTreeBuilder builder)
    {
        if (!ShowDirection)
            return;

        var page = PagerState.Page - 1;
        var disabledClass = PagerState.HasPreviousPage ? null : DisabledClass;

        RenderPageLink(builder, page, PreviousText, "Go to previous page", disabledClass);
    }

    /// <summary>
    /// Renders an individual pagination element as either a clickable button or static span.
    /// This method handles the rendering logic for all types of pagination links including
    /// page numbers, navigation arrows, and ellipsis indicators. It automatically applies
    /// appropriate accessibility attributes, CSS classes, and interactive behavior based on the element's state.
    /// </summary>
    /// <param name="builder">The render tree builder used to construct the pagination element.</param>
    /// <param name="page">The target page number for navigation when this element is clicked.</param>
    /// <param name="text">The display text or symbol shown within the pagination element.</param>
    /// <param name="title">The tooltip text displayed on hover, used for accessibility and user guidance.</param>
    /// <param name="disabledClass">The CSS class to apply for disabled or current page states.
    /// When null, the element is rendered as an interactive button.</param>
    private void RenderPageLink(RenderTreeBuilder builder, int page, string text, string title, string? disabledClass = null)
    {
        var enabled = string.IsNullOrEmpty(disabledClass);
        var isCurrent = disabledClass == CurrentClass;
        var isDisabled = disabledClass == DisabledClass;

        var itemClass = CssBuilder
            .Default(ItemClass)
            .AddClass(disabledClass, !enabled)
            .ToString();

        builder.OpenElement(5, "li");
        builder.AddAttribute(6, "class", itemClass);
        builder.AddAttribute(7, "aria-disabled", isDisabled);

        builder.SetKey(new { page, text });

        if (enabled)
        {
            builder.OpenElement(9, "button");
            builder.AddAttribute(10, "class", ButtonClass);
            builder.AddAttribute(11, "type", "button");
            builder.AddAttribute(12, "title", title);
            builder.AddAttribute(13, "aria-label", $"Page {page}");
            builder.AddAttribute(14, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => GoToPage(page)));
            builder.AddContent(15, text);
            builder.CloseElement(); // button
        }
        else
        {
            builder.OpenElement(16, "span");
            builder.AddAttribute(17, "class", ButtonClass);

            if (isCurrent)
                builder.AddAttribute(18, "aria-current", "page");

            if (isDisabled)
                builder.AddAttribute(19, "aria-hidden", true);

            builder.AddContent(20, text);
            builder.CloseElement(); // span
        }

        builder.CloseElement(); // li
    }

    /// <summary>
    /// Renders the next page navigation link with appropriate styling and accessibility attributes.
    /// This method creates a directional navigation element that allows users to move forward one page.
    /// The link is automatically disabled when on the last page.
    /// </summary>
    /// <param name="builder">The render tree builder used to construct the link element.</param>
    private void RenderNextLink(RenderTreeBuilder builder)
    {
        if (!ShowDirection)
            return;

        var page = PagerState.Page + 1;
        var disabledClass = PagerState.HasNextPage ? null : DisabledClass;

        RenderPageLink(builder, page, NextText, "Go to next page", disabledClass);
    }

    /// <summary>
    /// Renders the last page navigation link with appropriate styling and accessibility attributes.
    /// This method creates a boundary navigation element that allows users to jump to the final page.
    /// The link is automatically disabled when already on the last page.
    /// </summary>
    /// <param name="builder">The render tree builder used to construct the link element.</param>
    private void RenderLastLink(RenderTreeBuilder builder)
    {
        if (!ShowBoundary)
            return;

        var page = PagerState.PageCount;
        var disabledClass = PagerState.IsLastPage ? DisabledClass : null;

        RenderPageLink(builder, page, LastText, "Go to last page", disabledClass);
    }

    /// <summary>
    /// Calculates the optimal range of page numbers to display based on current configuration.
    /// This method implements the logic for determining which page links should be visible
    /// considering the <see cref="DisplaySize"/> limit and <see cref="CenterSelected"/> preference.
    /// It handles both centered and block-based pagination display modes.
    /// </summary>
    /// <returns>A tuple containing the start page number (inclusive) and end page number (inclusive)
    /// for the range of pages that should be displayed in the pagination controls.</returns>
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
    /// Handles property changes in the pager state and coordinates UI updates and event notifications.
    /// This method responds to changes in page number or page size by triggering component re-rendering
    /// and invoking the <see cref="PagerChanged"/> callback to notify parent components of pagination changes.
    /// All operations are executed asynchronously to ensure proper integration with Blazor's rendering cycle.
    /// </summary>
    /// <param name="sender">The object that raised the PropertyChanged event, typically the <see cref="PagerState"/>.</param>
    /// <param name="e">The event arguments containing the name of the property that changed.</param>
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
/// Provides data for pagination change events, containing information about the new page state.
/// This record is used by the <see cref="DataPager.PagerChanged"/> event to communicate
/// pagination changes to parent components, enabling coordinated data loading and state management.
/// </summary>
/// <param name="Page">The new current page number (1-based) after the pagination change.</param>
/// <param name="PageSize">The number of items displayed per page, used for calculating data offsets and ranges.</param>
public record PageChangedEventArgs(int Page, int PageSize);
