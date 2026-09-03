using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides a visual query builder for composing filter expressions.
/// </summary>
/// <remarks>
/// Supports dynamic fields, nested groups, and change notifications.
/// </remarks>
public partial class QueryBuilder : ComponentBase
{
    /// <summary>
    /// Gets or sets the content that defines the available query fields.
    /// </summary>
    /// <remarks>
    /// The content should include one or more <see cref="QueryBuilderTemplate"/> or <see cref="QueryBuilderField{TItem}"/> child components.
    /// </remarks>
    [Parameter]
    public RenderFragment? QueryFields { get; set; }

    /// <summary>
    /// Gets or sets the root query group representing the current filter expression.
    /// </summary>
    [Parameter]
    public QueryGroup? Query { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the query changes.
    /// </summary>
    [Parameter]
    public EventCallback<QueryGroup> QueryChanged { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether users can add query groups.
    /// </summary>
    [Parameter]
    public bool Groupable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the logical operator selector is displayed.
    /// </summary>
    [Parameter]
    public bool ShowLogic { get; set; } = true;

    /// <summary>
    /// Gets or sets the template for rendering the footer of the query builder.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets an externally owned collection of query fields.
    /// </summary>
    /// <remarks>
    /// When supplied, the host component is responsible for rendering <see cref="QueryFields"/> so the
    /// field definitions are available even when the query builder is not rendered.
    /// </remarks>
    [Parameter]
    public QueryFieldCollection? FieldCollection { get; set; }

    /// <summary>
    /// Gets the field collection in use, either <see cref="FieldCollection"/> or an internally owned one.
    /// </summary>
    public QueryFieldCollection CurrentFieldCollection { get; private set; } = new();

    /// <summary>
    /// Gets the collection of fields available for building queries.
    /// </summary>
    public List<QueryBuilderTemplate> Fields => CurrentFieldCollection.Fields;

    /// <summary>
    /// Raises <see cref="QueryChanged"/> and requests a UI refresh.
    /// </summary>
    public void Refresh()
    {
        QueryChanged.InvokeAsync(Query);
        StateHasChanged();
    }

    /// <summary>
    /// Gets or sets the root query group used by the builder.
    /// </summary>
    /// <remarks>
    /// Setting this property invokes <see cref="QueryChanged"/> when the value changes.
    /// </remarks>
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
        if (firstRender && Fields.Count == 0) // verify columns added
            throw new InvalidOperationException("QueryBuilder requires at least one query field child component.");

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // copy FieldCollection to CurrentFieldCollection if set
        if (FieldCollection != null)
            CurrentFieldCollection = FieldCollection;

        RootQuery ??= new QueryGroup();
    }

    /// <summary>
    /// Adds a field definition to the query builder.
    /// </summary>
    /// <param name="field">The <see cref="QueryBuilderTemplate"/> instance to add.</param>
    internal void AddField(QueryBuilderTemplate field)
        => CurrentFieldCollection.Add(field);

    /// <summary>
    /// Removes a field definition from the query builder.
    /// </summary>
    /// <param name="field">The <see cref="QueryBuilderTemplate"/> instance to remove.</param>
    internal void RemoveField(QueryBuilderTemplate field)
        => CurrentFieldCollection.Remove(field);
}
