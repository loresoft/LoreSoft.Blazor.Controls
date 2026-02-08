using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a filter editor for a single field in the <see cref="QueryBuilder{TItem}"/> component.
/// Allows users to select a field, operator, and value for building query/filter expressions.
/// </summary>
/// <typeparam name="TItem">The type of the data item being queried.</typeparam>
public partial class QueryBuilderFilter<TItem>
{
    /// <summary>
    /// Gets or sets the parent <see cref="QueryBuilder{TItem}"/> component.
    /// </summary>
    [CascadingParameter(Name = "QueryBuilder")]
    public required QueryBuilder<TItem> QueryBuilder { get; set; }

    /// <summary>
    /// Gets or sets the filter being edited.
    /// </summary>
    [Parameter, EditorRequired]
    public QueryFilter Filter { get; set; } = new();

    /// <summary>
    /// Gets or sets the parent query group, if this filter is part of a group.
    /// </summary>
    [Parameter]
    public QueryGroup? Parent { get; set; }

    /// <summary>
    /// Gets the field definition for the filter, if available.
    /// </summary>
    protected QueryBuilderField<TItem>? Field { get; set; }

    /// <summary>
    /// Gets the list of available fields from the parent <see cref="QueryBuilder{TItem}"/>.
    /// </summary>
    protected List<QueryBuilderField<TItem>> Fields => QueryBuilder.Fields;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        FieldChanged();
    }

    /// <summary>
    /// Updates the field definition when the filter's field changes and refreshes the query builder.
    /// </summary>
    protected void FieldChanged()
    {
        Field = Fields.FirstOrDefault(f => f.CurrentName == Filter.Field);
        QueryBuilder.Refresh();
        StateHasChanged();
    }

    /// <summary>
    /// Updates the filter value when the operator changes, especially for null-check operators, and refreshes the query builder.
    /// </summary>
    protected void OperatorChanged()
    {
        if (Filter.Operator == QueryOperators.IsNull || Filter.Operator == QueryOperators.IsNotNull)
            Filter.Value = null;

        QueryBuilder.Refresh();
        StateHasChanged();
    }

    /// <summary>
    /// Gets a value indicating whether the value input should be shown for the current operator.
    /// </summary>
    protected bool ShowValueInput
        => Filter.Operator is not QueryOperators.IsNull or QueryOperators.IsNotNull;

    /// <summary>
    /// Removes this filter from its parent group and refreshes the query builder.
    /// </summary>
    protected void DeleteFilter()
    {
        if (Parent == null)
            return;

        Parent.Filters.Remove(Filter);

        QueryBuilder.Refresh();
        StateHasChanged();
    }

    /// <summary>
    /// Gets the formatted value for display in the input control.
    /// </summary>
    /// <returns>The formatted value as a string.</returns>
    protected string? GetValue()
    {
        return Binding.Format(Filter.Value);
    }

    /// <summary>
    /// Sets the filter value from the input control and refreshes the query builder.
    /// </summary>
    /// <param name="args">The change event arguments containing the new value.</param>
    protected void SetValue(ChangeEventArgs args)
    {
        Filter.Value = Field != null ? Binding.Convert(args.Value, Field.Type ?? typeof(object)) : args.Value;
        QueryBuilder.Refresh();
        StateHasChanged();
    }
}
