using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents the state of a data pager, managing pagination information including current page position,
/// page size configuration, total item count, and computed navigation properties.
/// This class implements <see cref="INotifyPropertyChanged"/> to provide real-time notifications
/// when pagination state changes, enabling automatic UI updates in data-bound components.
/// Used by components like <see cref="DataPager"/>, <see cref="DataGrid{TItem}"/>, and <see cref="DataList{TItem}"/>
/// to coordinate pagination behavior across the application.
/// </summary>
public class DataPagerState : INotifyPropertyChanged
{
    private int _page;
    private int _pageSize;
    private int? _total;
    private string? _continuationToken;
    private string? _nextToken;

    /// <summary>
    /// Occurs when a property value changes.
    /// This event is raised whenever any of the pagination properties (<see cref="Page"/>, <see cref="PageSize"/>,
    /// or <see cref="Total"/>) are modified, allowing subscribers to respond to pagination state changes.
    /// Components typically use this event to trigger data refreshes or UI updates.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the current page number (1-based indexing).
    /// Valid values are from 1 to <see cref="PageCount"/>. Setting this property
    /// triggers the <see cref="PropertyChanged"/> event, allowing components to respond
    /// to page navigation. The value is automatically bounded within valid range by consuming components.
    /// </summary>
    /// <value>The current page number, with 1 representing the first page.</value>
    public int Page
    {
        get => _page;
        set => SetProperty(ref _page, value);
    }

    /// <summary>
    /// Gets or sets the number of items to display per page.
    /// When this property is modified, the current page is automatically reset to 1
    /// to prevent invalid state where the current page exceeds the new page count.
    /// Setting this property triggers the <see cref="PropertyChanged"/> event twice:
    /// once for the page reset and once for the page size change.
    /// </summary>
    /// <value>The number of items per page. Must be greater than 0 for meaningful pagination.</value>
    public int PageSize
    {
        get => _pageSize;
        set
        {
            Reset(); // when page size changes, go back to page 1
            SetProperty(ref _pageSize, value);
        }
    }

    /// <summary>
    /// Gets or sets the total number of items in the complete dataset.
    /// This value is used to calculate <see cref="PageCount"/> and determine
    /// the availability of navigation options. Setting this property triggers
    /// the <see cref="PropertyChanged"/> event and may affect computed properties
    /// like <see cref="HasNextPage"/> and <see cref="IsLastPage"/>.
    /// </summary>
    /// <value>The total count of items across all pages. A value of 0 indicates an empty dataset.</value>
    public int? Total
    {
        get => _total;
        set => SetProperty(ref _total, value);
    }

    public string? ContinuationToken
    {
        get => _continuationToken;
        set => SetProperty(ref _continuationToken, value);
    }

    public string? NextToken
    {
        get => _nextToken;
        set => SetProperty(ref _nextToken, value);
    }

    /// <summary>
    /// Gets the 1-based index of the first item displayed on the current page.
    /// This computed property provides the starting position for the current page's data range.
    /// For example, with a page size of 10, page 1 starts at item 1, page 2 starts at item 11, etc.
    /// Returns 0 when there are no items in the dataset.
    /// </summary>
    /// <value>The 1-based index of the first item on the current page, or 0 if no items exist.</value>
    public int StartItem
    {
        get
        {
            if (Total == 0)
                return 0;

            if (PageSize == 0)
                return 1;

            return Math.Max(EndItem - (PageSize - 1), 1);
        }
    }

    /// <summary>
    /// Gets the 1-based index of the last item displayed on the current page.
    /// This computed property provides the ending position for the current page's data range.
    /// The value is automatically bounded by the total item count, so the last page may
    /// contain fewer items than the configured page size.
    /// </summary>
    /// <value>The 1-based index of the last item on the current page, never exceeding <see cref="Total"/>.</value>
    public int EndItem
    {
        get
        {
            if (PageSize == 0)
                return Total ?? 0;

            return Math.Min(PageSize * Page, Total ?? 0);
        }
    }

    /// <summary>
    /// Gets the total number of pages required to display all items.
    /// This computed property is calculated by dividing the total item count by the page size
    /// and rounding up to ensure all items are included. Returns 0 when there are no items
    /// to display, which can be useful for conditional UI rendering.
    /// </summary>
    /// <value>The total number of pages needed to display all items, or 0 if no items exist.</value>
    public int PageCount
    {
        get
        {
            if (Total <= 0)
                return 0;

            return (int)Math.Ceiling((Total ?? 0) / (double)PageSize);
        }
    }



    /// <summary>
    /// Gets a value indicating whether a previous page exists before the current page.
    /// This computed property is useful for enabling or disabling "Previous" navigation buttons
    /// and determining whether backward navigation is possible. Returns false when on the first page.
    /// </summary>
    /// <value>true if the current page number is greater than 1; otherwise, false.</value>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Gets a value indicating whether a next page exists after the current page.
    /// This computed property is useful for enabling or disabling "Next" navigation buttons
    /// and determining whether forward navigation is possible. Returns false when on the last page
    /// or when there are no pages to display.
    /// </summary>
    /// <value>true if the current page number is less than the total page count; otherwise, false.</value>
    public bool HasNextPage => Page < PageCount;

    /// <summary>
    /// Gets a value indicating whether the current page is the first page in the pagination sequence.
    /// This computed property is commonly used for UI logic such as disabling "First" and "Previous"
    /// navigation buttons, or applying special styling to indicate the beginning of the dataset.
    /// </summary>
    /// <value>true if the current page is 1 or less; otherwise, false.</value>
    public bool IsFirstPage => Page <= 1;

    /// <summary>
    /// Gets a value indicating whether the current page is the last page in the pagination sequence.
    /// This computed property is commonly used for UI logic such as disabling "Last" and "Next"
    /// navigation buttons, or applying special styling to indicate the end of the dataset.
    /// Returns true when there are no pages to display.
    /// </summary>
    /// <value>true if the current page equals or exceeds the total page count; otherwise, false.</value>
    public bool IsLastPage => Page >= PageCount;

    /// <summary>
    /// Sets the initial page and page size values without triggering property change notifications.
    /// This internal method is used during component initialization to establish the baseline
    /// pagination state without causing unnecessary event notifications or UI updates.
    /// Typically called by pagination components during their setup phase.
    /// </summary>
    /// <param name="page">The initial page number to set (should be 1-based).</param>
    /// <param name="pageSize">The initial page size to set (should be greater than 0).</param>
    internal void Attach(int page, int pageSize)
    {
        _page = page;
        _pageSize = pageSize;
    }

    /// <summary>
    /// Resets the current page to 1 without triggering property change notifications.
    /// This internal method is used when the page size changes or when other operations
    /// require returning to the first page. It modifies the backing field directly
    /// to avoid recursive property change events during complex state updates.
    /// </summary>
    internal void Reset()
    {
        _page = 1;
        _continuationToken = null;
    }

    /// <summary>
    /// Sets a property value and raises the <see cref="PropertyChanged"/> event if the value has changed.
    /// This protected method implements the standard property change notification pattern,
    /// using <see cref="EqualityComparer{T}"/> to determine if the value has actually changed
    /// before triggering notifications. This prevents unnecessary event firing and UI updates.
    /// </summary>
    /// <typeparam name="T">The type of the property being set.</typeparam>
    /// <param name="field">A reference to the backing field that stores the property value.</param>
    /// <param name="value">The new value to assign to the property.</param>
    /// <param name="propertyName">The name of the property being changed.
    /// Automatically populated by the compiler when called from a property setter.</param>
    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        RaisePropertyChanged(propertyName);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// This protected virtual method can be overridden in derived classes to customize
    /// the property change notification behavior. It safely handles null event handlers
    /// and ensures that subscribers are notified of property changes in a thread-safe manner.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.
    /// This value is passed to event subscribers via <see cref="PropertyChangedEventArgs"/>.</param>
    protected virtual void RaisePropertyChanged(string? propertyName)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
