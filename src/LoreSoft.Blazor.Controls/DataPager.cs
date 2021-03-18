using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LoreSoft.Blazor.Controls.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls
{
    public class DataPager<TItem> : ComponentBase
    {
        private int? _pageSize;

        [CascadingParameter(Name = "Grid")]
        protected DataGrid<TItem> Grid { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> PagerAttributes { get; set; }

        [Parameter]
        public EventCallback<PageChangedEventArgs> PagerChanged { get; set; }


        [Parameter]
        public int PageSize
        {
            get => _pageSize ?? 20;
            set
            {
                // only allow set once
                if (_pageSize != null)
                    return;

                _ = SetPageSize(value);
            }
        }

        [Parameter]
        public int[] PageSizeOptions { get; set; } = { 10, 25, 50, 100 };

        [Parameter]
        public int DisplaySize { get; set; } = 5;


        [Parameter]
        public bool ShowPageSizeOption { get; set; } = true;

        [Parameter]
        public bool ShowPageInformation { get; set; } = true;


        [Parameter]
        public bool ShowBoundary { get; set; } = true;

        [Parameter]
        public bool ShowDirection { get; set; } = true;

        [Parameter]
        public bool ShowPage { get; set; } = true;

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
        public string PagerClass { get; set; } = "data-pagination";

        [Parameter]
        public string ItemClass { get; set; } = "data-page-item";

        [Parameter]
        public string ButtonClass { get; set; } = "data-page-link";

        [Parameter]
        public string CurrentClass { get; set; } = "active";

        [Parameter]
        public string DisabledClass { get; set; } = "disabled";


        [Parameter]
        public RenderFragment<DataPager<TItem>> InformationTemplate { get; set; }

        [Parameter]
        public RenderFragment<DataPager<TItem>> PageSizeTemplate { get; set; }


        public int Page { get; internal set; } = 1;

        public int TotalItems { get; internal set; }

        public int StartItem => PageSize == 0 ? 1 : EndItem - (PageSize - 1);

        public int EndItem => PageSize == 0 ? TotalItems : PageSize * Page;

        public int PageCount => TotalItems > 0 ? (int)Math.Ceiling(TotalItems / (double)PageSize) : 0;

        public bool HasPreviousPage => Page > 1;

        public bool HasNextPage => Page < PageCount;

        public bool IsFirstPage => Page <= 1;

        public bool IsLastPage => Page >= PageCount;


        public async Task FirstPage()
        {
            if (IsFirstPage)
                return;

            await GoToPage(1);
        }

        public async Task PreviousPage()
        {
            if (!HasPreviousPage)
                return;

            await GoToPage(Page - 1);
        }

        public async Task NextPage()
        {
            if (!HasNextPage)
                return;

            await GoToPage(Page + 1);
        }

        public async Task LastPage()
        {
            if (IsLastPage)
                return;

            await GoToPage(PageCount);
        }

        public async Task GoToPage(int page)
        {
            page = Math.Min(page, PageCount);
            page = Math.Max(page, 1);

            if (page == Page)
                return;

            Page = page;
            await OnChange();
        }


        public async Task SetPageSize(int size)
        {
            Console.WriteLine("DataPager.SetPageSize()");

            if (_pageSize == size)
                return;

            _pageSize = size;

            Page = 1;
            await OnChange();
        }


        protected async Task OnChange()
        {
            Console.WriteLine("DataPager.OnChange()");

            if (Grid != null)
                await Grid.RefreshAsync();
            else
                StateHasChanged();

            if (!PagerChanged.HasDelegate)
                return;

            var eventArgs = new PageChangedEventArgs(Page, PageSize);
            await PagerChanged.InvokeAsync(eventArgs);
        }

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("DataPager.OnInitializedAsync()");

            if (Grid == null)
                throw new InvalidOperationException("DataColumn<T> must be child of DataGrid");

            Grid.SetPager(this);
            await Grid.RefreshAsync();
        }

        protected override void OnParametersSet()
        {
            Console.WriteLine("DataPager.OnParametersSet()");

            base.OnParametersSet();
        }


        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (PageCount == 0)
                return;

            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "role", "navigation");
            builder.AddAttribute(2, "class", "data-pager");

            RenderPagination(builder);

            RenderPageSizeOptions(builder);

            RenderPageInformation(builder);

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
                    RenderPageLink(builder, p, p.ToString(), p == Page ? CurrentClass : null);

                if (end < PageCount)
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
            var disabledClass = IsFirstPage ? DisabledClass : null;

            RenderPageLink(builder, page, FirstText, disabledClass);
        }

        private void RenderPreviousLink(RenderTreeBuilder builder)
        {
            if (!ShowDirection)
                return;

            var page = Page - 1;
            var disabledClass = HasPreviousPage ? null : DisabledClass;

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

            var page = Page + 1;
            var disabledClass = HasNextPage ? null : DisabledClass;

            RenderPageLink(builder, page, NextText, disabledClass);
        }

        private void RenderLastLink(RenderTreeBuilder builder)
        {
            if (!ShowBoundary)
                return;

            var page = PageCount;
            var disabledClass = IsLastPage ? DisabledClass : null;

            RenderPageLink(builder, page, LastText, disabledClass);
        }

        private void RenderPageSizeOptions(RenderTreeBuilder builder)
        {
            if (!ShowPageSizeOption)
                return;

            builder.OpenElement(15, "div");
            builder.AddAttribute(16, "class", "data-page-size-options");

            builder.OpenElement(17, "select");
            builder.AddAttribute(18, "value", PageSize);
            builder.AddAttribute(19, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => SetPageSize(Convert.ToInt32(e.Value ?? 0))));

            foreach (var pageSizeOption in PageSizeOptions)
            {
                builder.OpenElement(20, "option");
                builder.AddAttribute(21, "value", pageSizeOption);
                builder.SetKey(pageSizeOption);
                builder.AddContent(22, pageSizeOption);
                builder.CloseElement(); // option
            }

            builder.OpenElement(23, "option");
            builder.AddAttribute(24, "value", 0);
            builder.AddContent(25, "All");
            builder.CloseElement(); // option


            builder.CloseElement(); // select

            builder.OpenElement(26, "span");
            builder.AddContent(27, "Items per page");
            builder.CloseElement(); // span

            builder.CloseElement(); // div
        }

        private void RenderPageInformation(RenderTreeBuilder builder)
        {
            if (!ShowPageInformation)
                return;

            builder.OpenElement(28, "div");
            builder.AddAttribute(29, "class", "data-page-information");

            if (InformationTemplate != null)
            {
                builder.AddContent(30, InformationTemplate(this));
            }
            else
            {
                builder.OpenElement(31, "span");
                builder.AddContent(32, $"{StartItem} - {EndItem} of {TotalItems}");
                builder.CloseElement(); // span
            }

            builder.CloseElement(); // div
        }


        private (int start, int end) GetPageEnds()
        {
            var start = 1;
            var end = PageCount;
            var isMax = DisplaySize > 0 && DisplaySize < PageCount;

            if (!isMax)
                return (start, end);

            if (CenterSelected)
            {
                int f = (int)Math.Floor(DisplaySize / 2d);

                start = Math.Max(Page - f, 1);
                end = start + DisplaySize - 1;

                if (end <= PageCount)
                    return (start, end);

                end = PageCount;
                start = end - DisplaySize + 1;

                return (start, end);
            }


            int c = (int)Math.Ceiling(Page / (double)DisplaySize);

            start = (c - 1) * DisplaySize + 1;
            end = Math.Min(start + DisplaySize - 1, PageCount);
            return (start, end);
        }
    }

    public record PageChangedEventArgs(int Page, int PageSize);
}
