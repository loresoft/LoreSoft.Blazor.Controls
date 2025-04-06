using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

[CascadingTypeParameter(nameof(TItem))]
public partial class QueryBuilder<TItem> : ComponentBase
{
    [Parameter]
    public RenderFragment? QueryFields { get; set; }

    [Parameter]
    public QueryGroup? Query { get; set; }

    [Parameter]
    public EventCallback<QueryGroup> QueryChanged { get; set; }

    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }


    public List<QueryBuilderField<TItem>> Fields { get; } = [];

    public void Refresh()
    {
        QueryChanged.InvokeAsync(Query);
        StateHasChanged();
    }

    protected QueryGroup? RootQuery
    {
        get => Query;
        set
        {
            if (Query == value)
                return;

            Query = value;
            _ = QueryChanged.InvokeAsync(value);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && (Fields == null || Fields.Count == 0)) // verify columns added
            throw new InvalidOperationException("QueryBuilder requires at least one QueryColumn child component.");

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        RootQuery ??= new QueryGroup();
    }

    internal void AddField(QueryBuilderField<TItem> column)
    {
        Fields.Add(column);
    }
}
