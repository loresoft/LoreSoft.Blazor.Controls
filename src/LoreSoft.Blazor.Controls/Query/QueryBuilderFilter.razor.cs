using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents the editor for a single filter within a <see cref="QueryBuilder"/>.
/// </summary>
/// <remarks>
/// Allows users to choose a field, operator, and value when composing query expressions.
/// </remarks>
public partial class QueryBuilderFilter
{
    /// <summary>
    /// Gets or sets the parent <see cref="QueryBuilder"/> component.
    /// </summary>
    [CascadingParameter(Name = "QueryBuilder")]
    public required QueryBuilder QueryBuilder { get; set; }

    /// <summary>
    /// Gets or sets the filter currently being edited.
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
    protected QueryBuilderTemplate? Field { get; set; }

    /// <summary>
    /// Gets the list of available fields from the parent <see cref="QueryBuilder"/>.
    /// </summary>
    protected List<QueryBuilderTemplate> Fields => QueryBuilder.Fields;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        FieldChanged();
    }

    /// <summary>
    /// Updates the selected field metadata when the filter field changes.
    /// </summary>
    /// <remarks>
    /// Refreshes the parent <see cref="QueryBuilder"/> after updating the selected field.
    /// </remarks>
    protected void FieldChanged()
    {
        Field = Fields.FirstOrDefault(f => f.CurrentName == Filter.Field);
        QueryBuilder.Refresh();
        StateHasChanged();
    }

    /// <summary>
    /// Handles updates when the selected operator changes.
    /// </summary>
    /// <remarks>
    /// Clears <see cref="QueryFilter.Value"/> for null-check operators and refreshes the parent <see cref="QueryBuilder"/>.
    /// </remarks>
    protected void OperatorChanged()
    {
        if (Filter.Operator == QueryOperators.IsNull || Filter.Operator == QueryOperators.IsNotNull)
            Filter.Value = null;

        QueryBuilder.Refresh();
        StateHasChanged();
    }

    /// <summary>
    /// Gets a value indicating whether the value input is visible for the current operator.
    /// </summary>
    protected bool ShowValueInput
        => Filter.Operator is not (QueryOperators.IsNull or QueryOperators.IsNotNull);

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
    /// Sets the filter value from the input control.
    /// </summary>
    /// <param name="args">The change event arguments containing the new value.</param>
    /// <remarks>
    /// Converts the incoming value to the selected field type when available, then refreshes the parent <see cref="QueryBuilder"/>.
    /// </remarks>
    protected void SetValue(ChangeEventArgs args)
    {
        Filter.Value = Field != null ? Binding.Convert(args.Value, Field.Type ?? typeof(object)) : args.Value;
        QueryBuilder.Refresh();
        StateHasChanged();
    }
}
