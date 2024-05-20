using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoreSoft.Blazor.Controls;

public class DataPagerState
{
    private int _page;
    private int _pageSize;
    private int _total;


    public event PropertyChangedEventHandler PropertyChanged;


    public int Page
    {
        get => _page;
        set => SetProperty(ref _page, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            Reset(); // when page size changes, go back to page 1
            SetProperty(ref _pageSize, value);
        }
    }

    public int Total
    {
        get => _total;
        set => SetProperty(ref _total, value);
    }


    public int StartItem => PageSize == 0 ? 1 : Math.Max(EndItem - (PageSize - 1), 0);

    public int EndItem => PageSize == 0 ? Total : Math.Min(PageSize * Page, Total);


    public int PageCount => Total > 0 ? (int)Math.Ceiling(Total / (double)PageSize) : 0;


    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < PageCount;

    public bool IsFirstPage => Page <= 1;

    public bool IsLastPage => Page >= PageCount;


    internal void Attach(int page, int pageSize)
    {
        _page = page;
        _pageSize = pageSize;
    }

    internal void Reset()
    {
        _page = 1;
    }


    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        RaisePropertyChanged(propertyName);
    }

    protected virtual void RaisePropertyChanged(string propertyName)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}