using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a group editor within the <see cref="QueryBuilder"/> component.
/// </summary>
/// <remarks>
/// Supports nested logical groups (AND/OR) composed of filters and subgroups.
/// </remarks>
public partial class QueryBuilderGroup
{
    /// <summary>
    /// Gets or sets the parent <see cref="QueryBuilder"/> component.
    /// </summary>
    [CascadingParameter(Name = "QueryBuilder")]
    public required QueryBuilder QueryBuilder { get; set; }

    /// <summary>
    /// Gets or sets the query group represented by this editor.
    /// </summary>
    [Parameter, EditorRequired]
    public required QueryGroup Group { get; set; } = new();

    /// <summary>
    /// Gets or sets the parent query group, if this group is nested.
    /// </summary>
    [Parameter]
    public QueryGroup? Parent { get; set; }

    /// <summary>
    /// Removes this group from its parent group.
    /// </summary>
    /// <remarks>
    /// Refreshes the parent <see cref="QueryBuilder"/> after removal.
    /// </remarks>
    protected void DeleteGroup()
    {
        if (Parent == null)
            return;

        Parent.Filters.Remove(Group);

        QueryBuilder.Refresh();
    }

    /// <summary>
    /// Adds a new subgroup to this group.
    /// </summary>
    /// <remarks>
    /// Refreshes the parent <see cref="QueryBuilder"/> after adding the subgroup.
    /// </remarks>
    protected void AddGroup()
    {
        if (Group == null)
            return;

        var group = new QueryGroup();
        Group.Filters.Add(group);

        QueryBuilder.Refresh();
    }

    /// <summary>
    /// Adds a new filter to this group.
    /// </summary>
    /// <remarks>
    /// Refreshes the parent <see cref="QueryBuilder"/> after adding the filter.
    /// </remarks>
    protected void AddFilter()
    {
        if (Group == null)
            return;

        var group = new QueryFilter();
        Group.Filters.Add(group);

        QueryBuilder.Refresh();
    }
}
