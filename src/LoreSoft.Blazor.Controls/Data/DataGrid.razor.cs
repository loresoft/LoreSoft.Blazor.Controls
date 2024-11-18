using System;
using System.Linq.Dynamic.Core;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

[CascadingTypeParameter(nameof(TItem))]
public partial class DataGrid<TItem> : DataComponentBase<TItem>
{
    private HashSet<TItem> _expandedItems = new();
    private HashSet<string> _expandedGroups = new();

    private QueryGroup _query;

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
    public bool Filterable { get; set; }

    [Parameter]
    public bool Groupable { get; set; }

    [Parameter]
    public string TableClass { get; set; } = "table";

    [Parameter]
    public string RowClass { get; set; }

    [Parameter]
    public Func<TItem, string> RowStyle { get; set; }

    [Parameter]
    public Func<TItem, Dictionary<string, object>> RowAttributes { get; set; }

    [Parameter]
    public IEnumerable<TItem> SelectedItems { get; set; } = [];

    [Parameter]
    public EventCallback<IEnumerable<TItem>> SelectedItemsChanged { get; set; }

    [Parameter]
    public EventCallback<TItem> RowDoubleClick { get; set; }

    [Parameter]
    public QueryGroup Query { get; set; }

    protected QueryGroup RootQuery { get; set; }


    public List<DataColumn<TItem>> Columns { get; } = [];


    protected bool FilterOpen { get; set; }

    protected void ShowFilter()
    {
        if (RootQuery.Filters.Count == 0)
            RootQuery.Filters.Add(new QueryFilter());

        FilterOpen = true;
    }

    protected void CloseFilter()
    {
        FilterOpen = false;
    }

    protected bool IsFilterActive()
    {
        return LinqExpressionBuilder.IsValid(RootQuery);
    }

    protected async Task ApplyFilters()
    {
        FilterOpen = false;
        await RefreshAsync(true);
    }


    public async Task QuickSearch(string searchText, bool clearFilter = false)
    {
        if (clearFilter)
            RootQuery.Filters.Clear();
        else
            RootQuery.Filters.RemoveAll(f => f.Id == nameof(QuickSearch));

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var quickSearch = new QueryGroup { Id = nameof(QuickSearch), Logic = QueryLogic.Or };

            // all filterable string columns
            foreach (var column in Columns.Where(c => c.Filterable && c.PropertyType == typeof(string)))
            {
                var filter = new QueryFilter
                {
                    Field = column.ColumnName,
                    Operator = QueryOperators.Contains,
                    Value = searchText
                };
                quickSearch.Filters.Add(filter);
            }

            RootQuery.Filters.Add(quickSearch);
        }

        await RefreshAsync(true);
    }

    public async Task ClearFilters()
    {
        RootQuery.Filters.Clear();
        FilterOpen = false;
        await RefreshAsync(true);
    }

    public async Task ApplyFilters(IEnumerable<QueryRule> rules, bool replace = false)
    {
        if (replace)
            RootQuery.Filters.Clear();

        if (rules != null)
            RootQuery.Filters.AddRange(rules);

        await RefreshAsync(true);
    }

    public async Task ApplyFilter(QueryRule rule)
    {
        if (rule == null)
            return;

        if (rule.Id.HasValue())
            RootQuery.Filters.RemoveAll(f => f.Id == rule.Id);

        RootQuery.Filters.Add(rule);
        await RefreshAsync(true);
    }

    public override async Task RefreshAsync(bool resetPager = false, bool forceReload = false)
    {
        // clear row flags on refresh
        _expandedItems.Clear();
        SetSelectedItems(new List<TItem>());

        await base.RefreshAsync(resetPager, forceReload);
    }


    public async Task SortByAsync(DataColumn<TItem> column)
    {
        if (column == null || !Sortable || !column.Sortable)
            return;

        var descending = column.CurrentSortIndex >= 0 && !column.CurrentSortDescending;

        Columns.ForEach(c => c.UpdateSort(-1, false));

        column.UpdateSort(0, descending);

        await RefreshAsync();
    }

    public async Task SortByAsync(string columnName)
    {
        if (!Sortable)
            return;

        var column = Columns.Find(c => c.ColumnName == columnName);
        await SortByAsync(column);
    }


    protected List<DataColumn<TItem>> VisibleColumns => Columns.Where(c => c.Visible).ToList();

    protected int CellCount => (Columns?.Count ?? 0)
        + (DetailTemplate != null || (Groupable && Columns.Any(c => c.Grouping)) ? 1 : 0)
        + (Selectable ? 1 : 0);


    protected bool HasGrouping => Groupable && Columns.Any(c => c.Grouping);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && (Columns == null || Columns.Count == 0)) // verify columns added
            throw new InvalidOperationException("DataGrid requires at least one DataColumn child component.");

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // only update RootQuery if Query changed
        if (_query != Query)
        {
            _query = Query;
            RootQuery = Query;
        }

        RootQuery ??= new QueryGroup();
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


    protected bool IsGroupExpanded(string key)
    {
        return _expandedGroups.Contains(key);
    }

    protected void ToggleGroupRow(string key)
    {
        if (_expandedGroups.Contains(key))
            _expandedGroups.Remove(key);
        else
            _expandedGroups.Add(key);

        StateHasChanged();
    }


    protected bool IsAllSelected()
    {
        if (View == null || View.Count == 0)
            return false;

        return View.All(IsRowSelected);
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


    public override DataRequest CreateDataRequest(CancellationToken cancellationToken = default)
    {
        var sorts = Columns
            .Where(c => c.CurrentSortIndex >= 0)
            .OrderBy(c => c.CurrentSortIndex)
            .Select(c => new DataSort(c.ColumnName, c.CurrentSortDescending))
            .ToArray();

        return new DataRequest(Pager.Page, Pager.PageSize, sorts, RootQuery, cancellationToken);
    }

    protected override IQueryable<TItem> FilterData(IQueryable<TItem> queryable, DataRequest request)
    {
        if (!Filterable || !LinqExpressionBuilder.IsValid(RootQuery))
            return queryable;

        return queryable.Filter(request.Query);
    }

    protected override IQueryable<TItem> SortData(IQueryable<TItem> queryable, DataRequest request)
    {
        if (!Sortable || request.Sorts == null || request.Sorts.Length == 0)
            return queryable;

        var columns = request.Sorts.Select(s => s.Property);

        var sorted = Columns
            .Where(c => columns.Contains(c.ColumnName))
            .OrderBy(c => c.CurrentSortIndex)
            .ToList();

        if (sorted.Count == 0)
            return queryable;

        var queue = new Queue<DataColumn<TItem>>(sorted);
        var column = queue.Dequeue();

        var orderedQueryable = column.CurrentSortDescending
            ? queryable.OrderByDescending(column.Property)
            : queryable.OrderBy(column.Property);

        while (queue.Count > 0)
        {
            column = queue.Dequeue();

            orderedQueryable = column.CurrentSortDescending
                ? orderedQueryable.ThenByDescending(column.Property)
                : orderedQueryable.ThenBy(column.Property);
        }

        return orderedQueryable;
    }
}
