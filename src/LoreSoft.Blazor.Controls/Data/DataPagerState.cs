using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents the state of a data pager, including current page, page size, total items, and navigation properties.
/// Notifies listeners when properties change.
/// </summary>
public class DataPagerState
{
    private int _page;
    private int _pageSize;
    private int _total;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int Page
    {
        get => _page;
        set => SetProperty(ref _page, value);
    }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// Setting this property resets the page to 1.
    /// </summary>
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
    /// Gets or sets the total number of items.
    /// </summary>
    public int Total
    {
        get => _total;
        set => SetProperty(ref _total, value);
    }

    /// <summary>
    /// Gets the index of the first item on the current page (1-based).
    /// </summary>
    public int StartItem => PageSize == 0 ? 1 : Math.Max(EndItem - (PageSize - 1), 1);

    /// <summary>
    /// Gets the index of the last item on the current page (1-based).
    /// </summary>
    public int EndItem => PageSize == 0 ? Total : Math.Min(PageSize * Page, Total);

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int PageCount => Total > 0 ? (int)Math.Ceiling(Total / (double)PageSize) : 0;

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < PageCount;

    /// <summary>
    /// Gets a value indicating whether the current page is the first page.
    /// </summary>
    public bool IsFirstPage => Page <= 1;

    /// <summary>
    /// Gets a value indicating whether the current page is the last page.
    /// </summary>
    public bool IsLastPage => Page >= PageCount;

    /// <summary>
    /// Sets the initial page and page size values.
    /// </summary>
    /// <param name="page">The page number to set.</param>
    /// <param name="pageSize">The page size to set.</param>
    internal void Attach(int page, int pageSize)
    {
        _page = page;
        _pageSize = pageSize;
    }

    /// <summary>
    /// Resets the current page to 1.
    /// </summary>
    internal void Reset()
    {
        _page = 1;
    }

    /// <summary>
    /// Sets the property value and raises <see cref="PropertyChanged"/> if the value changes.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">The backing field reference.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The name of the property.</param>
    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        RaisePropertyChanged(propertyName);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void RaisePropertyChanged(string? propertyName)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
