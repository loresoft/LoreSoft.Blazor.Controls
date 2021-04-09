using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls
{
    public partial class DataGrid<TItem> : DataComponentBase<TItem>
    {
        private HashSet<TItem> _expandedItems = new();

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> TableAttributes { get; set; }


        [Parameter]
        public RenderFragment DataColumns { get; set; }

        [Parameter]
        public RenderFragment<TItem> DetailTemplate { get; set; }


        [Parameter]
        public bool Selectable { get; set; }

        [Parameter]
        public bool Sortable { get; set; } = true;


        [Parameter]
        public string TableClass { get; set; } = "table";

        [Parameter]
        public string RowClass { get; set; }

        [Parameter]
        public Func<TItem, string> RowStyle { get; set; }


        [Parameter]
        public IEnumerable<TItem> SelectedItems { get; set; } = new List<TItem>();

        [Parameter]
        public EventCallback<IEnumerable<TItem>> SelectedItemsChanged { get; set; }


        public List<DataColumn<TItem>> Columns { get; } = new();


        public override async Task RefreshAsync(bool resetPager = false)
        {
            // clear row flags on refresh
            _expandedItems.Clear();
            SetSelectedItems(new List<TItem>());

            await base.RefreshAsync(resetPager);
        }


        public async Task SortByAsync(DataColumn<TItem> column)
        {
            if (column == null || !Sortable || !column.Sortable)
                return;

            var descending = column.SortIndex >= 0 && !column.SortDescending;

            Columns.ForEach(c => c.UpdateSort(-1, false));

            column.UpdateSort(0, descending);

            await RefreshAsync();
        }

        public async Task SortByAsync(string columnName)
        {
            if (!Sortable)
                return;

            var column = Columns.Find(c => c.Name == columnName);
            await SortByAsync(column);
        }


        protected List<DataColumn<TItem>> VisibleColumns => Columns.Where(c => c.Visible).ToList();

        protected int CellCount => (Columns?.Count ?? 0) + (DetailTemplate != null ? 1 : 0) + (Selectable ? 1 : 0);


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && (Columns == null || Columns.Count == 0)) // verify columns added
                throw new InvalidOperationException("DataGrid requires at least one DataColumn child component.");

            await base.OnAfterRenderAsync(firstRender);
        }


        protected bool IsRowExpanded(TItem item)
        {
            return _expandedItems.Contains(item);
        }

        protected void ToggleDetailRow(TItem item)
        {
            if (_expandedItems.Contains(item))
                _expandedItems.Remove(item);
            else
                _expandedItems.Add(item);

            StateHasChanged();
        }


        protected bool IsAllSelected()
        {
            return View?.All(IsRowSelected) == true;
        }

        protected void ToggleSelectAll()
        {
            if (IsAllSelected())
                DeselectAll();
            else
                SelectAll();
        }

        protected bool IsRowSelected(TItem item)
        {
            return SelectedItems?.Contains(item) ?? false;
        }

        protected void ToggleSelectRow(TItem item)
        {
            var list = new List<TItem>(SelectedItems ?? Enumerable.Empty<TItem>());
            if (IsRowSelected(item))
            {
                list.Remove(item);
                SetSelectedItems(list);
            }
            else
            {
                list.Add(item);
                SetSelectedItems(list);
            }

            StateHasChanged();
        }

        protected void SelectAll()
        {
            var list = View?.ToList() ?? new List<TItem>();
            SetSelectedItems(list);
            StateHasChanged();
        }

        protected void DeselectAll()
        {
            SetSelectedItems(new List<TItem>());
            StateHasChanged();
        }

        protected async void SetSelectedItems(IEnumerable<TItem> items)
        {
            SelectedItems = items;
            await SelectedItemsChanged.InvokeAsync(SelectedItems);
        }


        internal void AddColumn(DataColumn<TItem> column)
        {
            Columns.Add(column);
        }


        protected override DataRequest CreateDataRequest(CancellationToken cancellationToken)
        {
            var sorts = Columns
                .Where(c => c.SortIndex >= 0)
                .OrderBy(c => c.SortIndex)
                .Select(c => new DataSort(c.Name, c.SortDescending))
                .ToArray();

            return new DataRequest(Pager.Page, Pager.PageSize, sorts, cancellationToken);
        }

        protected override IQueryable<TItem> SortData(IQueryable<TItem> queryable, DataRequest request)
        {
            if (!Sortable || request.Sorts == null || request.Sorts.Length == 0)
                return queryable;

            var columns = request.Sorts.Select(s => s.Property);

            var sorted = Columns
                .Where(c => columns.Contains(c.Name))
                .OrderBy(c => c.SortIndex)
                .ToList();

            if (sorted.Count == 0)
                return queryable;

            var queue = new Queue<DataColumn<TItem>>(sorted);
            var column = queue.Dequeue();

            var orderedQueryable = column.SortDescending
                ? queryable.OrderByDescending(column.Property)
                : queryable.OrderBy(column.Property);

            while (queue.Count > 0)
            {
                column = queue.Dequeue();

                orderedQueryable = column.SortDescending
                    ? orderedQueryable.ThenByDescending(column.Property)
                    : orderedQueryable.ThenBy(column.Property);
            }

            return orderedQueryable;
        }
    }

}
