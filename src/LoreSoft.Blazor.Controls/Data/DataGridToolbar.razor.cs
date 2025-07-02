using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;
public partial class DataGridToolbar<TItem> : ComponentBase
    where TItem : class
{
    public DataGridToolbar()
    {
        SearchText = new DebounceValue<string>(HandleSearch);
    }

    [Inject]
    public required IJSRuntime JavaScript { get; set; }

    [CascadingParameter(Name = "Grid")]
    protected DataGrid<TItem>? ParentGrid { get; set; }

    [Parameter]
    public DataGrid<TItem>? DataGrid { get; set; }

    [Parameter]
    public RenderFragment<DataGrid<TItem>>? TitleTemplate { get; set; }

    [Parameter]
    public RenderFragment<DataGrid<TItem>>? FilterTemplate { get; set; }

    [Parameter]
    public RenderFragment<DataGrid<TItem>>? ActionTemplate { get; set; }


    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? ExportName { get; set; }


    [Parameter]
    public bool ShowSearch { get; set; } = true;

    [Parameter]
    public bool ShowFilter { get; set; } = true;

    [Parameter]
    public bool ShowExport { get; set; } = true;

    [Parameter]
    public bool ShowRefresh { get; set; } = true;

    [Parameter]
    public bool ShowColumnPicker { get; set; } = true;


    protected DataGrid<TItem>? CurrentGrid { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        CurrentGrid = DataGrid ?? ParentGrid;
    }

    protected DebounceValue<string> SearchText { get; }

    protected void HandleSearch(string? searchText)
    {
        if (CurrentGrid == null)
            return;

        InvokeAsync(() => CurrentGrid.QuickSearch(searchText));
    }

    protected async Task HandelRefresh()
    {
        if (CurrentGrid == null)
            return;

        await CurrentGrid.RefreshAsync();
    }

    protected async Task HandelExport()
    {
        if (CurrentGrid == null)
            return;

        var name = ExportName ?? Title ?? "Export";
        var fileName = $"{name} {DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv";
        await CurrentGrid.ExportAsync(fileName);
    }

    protected void ToggleFilter()
    {
        if (CurrentGrid == null)
            return;

        CurrentGrid.ShowFilter();
    }

    protected void ToggleColumnPicker()
    {
        if (CurrentGrid == null)
            return;

        CurrentGrid.ShowColumnPicker();
    }

}
