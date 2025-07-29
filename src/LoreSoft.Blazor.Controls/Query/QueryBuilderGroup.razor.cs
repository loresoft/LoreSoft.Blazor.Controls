using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a group of query rules in the <see cref="QueryBuilder{TItem}"/> component.
/// Allows users to build nested logical groups (AND/OR) of filters and subgroups for complex query expressions.
/// </summary>
/// <typeparam name="TItem">The type of the data item being queried.</typeparam>
public partial class QueryBuilderGroup<TItem>
{
    /// <summary>
    /// Gets or sets the parent <see cref="QueryBuilder{TItem}"/> component.
    /// </summary>
    [CascadingParameter(Name = "QueryBuilder")]
    public required QueryBuilder<TItem> QueryBuilder { get; set; }

    /// <summary>
    /// Gets or sets the query group represented by this component.
    /// </summary>
    [Parameter, EditorRequired]
    public required QueryGroup Group { get; set; } = new();

    /// <summary>
    /// Gets or sets the parent query group, if this group is nested.
    /// </summary>
    [Parameter]
    public QueryGroup? Parent { get; set; }

    /// <summary>
    /// Removes this group from its parent group and refreshes the query builder.
    /// </summary>
    protected void DeleteGroup()
    {
        if (Parent == null)
            return;

        Parent.Filters.Remove(Group);

        QueryBuilder.Refresh();
    }

    /// <summary>
    /// Adds a new subgroup to this group and refreshes the query builder.
    /// </summary>
    protected void AddGroup()
    {
        if (Group == null)
            return;

        var group = new QueryGroup();
        Group.Filters.Add(group);

        QueryBuilder.Refresh();
    }

    /// <summary>
    /// Adds a new filter to this group and refreshes the query builder.
    /// </summary>
    protected void AddFilter()
    {
        if (Group == null)
            return;

        var group = new QueryFilter();
        Group.Filters.Add(group);

        QueryBuilder.Refresh();
    }
}
