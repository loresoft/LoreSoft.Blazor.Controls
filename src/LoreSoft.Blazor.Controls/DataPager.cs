using System;
using System.Collections.Generic;
using System.ComponentModel;
using LoreSoft.Blazor.Controls.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls;

public class DataPager : ComponentBase, IDisposable
{
    [CascadingParameter(Name = "PagerState")]
    protected DataPagerState PagerState { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; }


    [Parameter]
    public EventCallback<PageChangedEventArgs> PagerChanged { get; set; }


    [Parameter]
    public int PageSize { get; set; } = 25;

    [Parameter]
    public int DisplaySize { get; set; } = 5;

    [Parameter]
    public bool ShowBoundary { get; set; } = true;

    [Parameter]
    public bool ShowDirection { get; set; } = true;

    [Parameter]
    public bool ShowPage { get; set; } = true;

    [Parameter]
    public bool ShowEmpty { get; set; } = false;

    [Parameter]
    public bool CenterSelected { get; set; } = true;

    [Parameter]
    public string PreviousText { get; set; } = "‹";

    [Parameter]
    public string NextText { get; set; } = "›";

    [Parameter]
    public string FirstText { get; set; } = "«";

    [Parameter]
    public string LastText { get; set; } = "»";

    [Parameter]
    public string PagerClass { get; set; } = "data-pager";

    [Parameter]
    public string ItemClass { get; set; } = "data-page-item";

    [Parameter]
    public string ButtonClass { get; set; } = "data-page-link";

    [Parameter]
    public string CurrentClass { get; set; } = "active";

    [Parameter]
    public string DisabledClass { get; set; } = "disabled";


    public void FirstPage()
    {
        if (PagerState.IsFirstPage)
            return;

        GoToPage(1);
    }

    public void PreviousPage()
    {
        if (!PagerState.HasPreviousPage)
            return;

        GoToPage(PagerState.Page - 1);
    }

    public void NextPage()
    {
        if (!PagerState.HasNextPage)
            return;

        GoToPage(PagerState.Page + 1);
    }

    public void LastPage()
    {
        if (PagerState.IsLastPage)
            return;

        GoToPage(PagerState.PageCount);
    }

    public void GoToPage(int page)
    {
        page = Math.Min(page, PagerState.PageCount);
        page = Math.Max(page, 1);

        if (page == PagerState.Page)
            return;

        PagerState.Page = page;
    }


    public void Dispose()
    {
        PagerState.PropertyChanged -= OnStatePropertyChange;
    }


    protected override void OnInitialized()
    {
        if (PagerState == null)
            throw new InvalidOperationException("DataSizer requires a cascading parameter PagerState.");

        // copy defaults to state
        PagerState.Attach(1, PageSize);
        PagerState.PropertyChanged += OnStatePropertyChange;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (PagerState.PageCount == 0 && !ShowEmpty)
            return;

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "role", "navigation");
        builder.AddMultipleAttributes(2, Attributes);

        RenderPagination(builder);

        builder.CloseElement(); // div

    }


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
                RenderPageLink(builder, start - 1, "...");

            for (var p = start; p <= end; p++)
                RenderPageLink(builder, p, p.ToString(), p == PagerState.Page ? CurrentClass : null);

            if (end < PagerState.PageCount)
                RenderPageLink(builder, end + 1, "...");
        }

        RenderNextLink(builder);
        RenderLastLink(builder);

        builder.CloseElement(); // ul
    }

    private void RenderFirstLink(RenderTreeBuilder builder)
    {
        if (!ShowBoundary)
            return;

        var page = 1;
        var disabledClass = PagerState.IsFirstPage ? DisabledClass : null;

        RenderPageLink(builder, page, FirstText, disabledClass);
    }

    private void RenderPreviousLink(RenderTreeBuilder builder)
    {
        if (!ShowDirection)
            return;

        var page = PagerState.Page - 1;
        var disabledClass = PagerState.HasPreviousPage ? null : DisabledClass;

        RenderPageLink(builder, page, PreviousText, disabledClass);
    }

    private void RenderPageLink(RenderTreeBuilder builder, int page, string text, string disabledClass = null)
    {
        if (!ShowPage)
            return;

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
            builder.AddAttribute(10, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => GoToPage(page)));
            builder.AddContent(11, text);
            builder.CloseElement(); // button
        }
        else
        {
            builder.OpenElement(12, "span");
            builder.AddAttribute(13, "class", ButtonClass);
            builder.AddContent(14, text);
            builder.CloseElement(); // span
        }

        builder.CloseElement(); // li
    }

    private void RenderNextLink(RenderTreeBuilder builder)
    {
        if (!ShowDirection)
            return;

        var page = PagerState.Page + 1;
        var disabledClass = PagerState.HasNextPage ? null : DisabledClass;

        RenderPageLink(builder, page, NextText, disabledClass);
    }

    private void RenderLastLink(RenderTreeBuilder builder)
    {
        if (!ShowBoundary)
            return;

        var page = PagerState.PageCount;
        var disabledClass = PagerState.IsLastPage ? DisabledClass : null;

        RenderPageLink(builder, page, LastText, disabledClass);
    }

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


    private void OnStatePropertyChange(object sender, PropertyChangedEventArgs e)
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

public record PageChangedEventArgs(int Page, int PageSize);
