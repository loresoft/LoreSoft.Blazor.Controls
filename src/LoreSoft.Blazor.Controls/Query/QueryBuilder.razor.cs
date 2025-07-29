using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component for building complex query/filter expressions using a visual interface.
/// Supports dynamic fields, nested groups, and change notification.
/// </summary>
/// <typeparam name="TItem">The type of the data item being queried.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public partial class QueryBuilder<TItem> : ComponentBase
{
    /// <summary>
    /// Gets or sets the template for defining available query fields.
    /// Should contain one or more <see cref="QueryBuilderField{TItem}"/> child components.
    /// </summary>
    [Parameter]
    public RenderFragment? QueryFields { get; set; }

    /// <summary>
    /// Gets or sets the root query group representing the current filter expression.
    /// </summary>
    [Parameter]
    public QueryGroup? Query { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the query changes.
    /// </summary>
    [Parameter]
    public EventCallback<QueryGroup> QueryChanged { get; set; }

    /// <summary>
    /// Gets or sets the template for rendering the footer of the query builder.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Gets the list of fields available for building queries.
    /// </summary>
    public List<QueryBuilderField<TItem>> Fields { get; } = [];

    /// <summary>
    /// Triggers the <see cref="QueryChanged"/> event and updates the UI.
    /// </summary>
    public void Refresh()
    {
        QueryChanged.InvokeAsync(Query);
        StateHasChanged();
    }

    /// <summary>
    /// Gets or sets the root query group for the builder.
    /// Setting this property triggers <see cref="QueryChanged"/> if the value changes.
    /// </summary>
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

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && (Fields == null || Fields.Count == 0)) // verify columns added
            throw new InvalidOperationException("QueryBuilder requires at least one QueryColumn child component.");

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        RootQuery ??= new QueryGroup();
    }

    /// <summary>
    /// Adds a field to the query builder's field list.
    /// </summary>
    /// <param name="column">The <see cref="QueryBuilderField{TItem}"/> to add.</param>
    internal void AddField(QueryBuilderField<TItem> column)
    {
        Fields.Add(column);
    }
}
