using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public partial class QueryBuilderGroup<TItem>
{
    [CascadingParameter(Name = "QueryBuilder")]
    public required QueryBuilder<TItem> QueryBuilder { get; set; }

    [Parameter, EditorRequired]
    public required QueryGroup Group { get; set; } = new();

    [Parameter]
    public QueryGroup Parent { get; set; }

    protected void DeleteGroup()
    {
        if (Parent == null)
            return;

        Parent.Filters.Remove(Group);

        QueryBuilder.Refresh();
    }

    protected void AddGroup()
    {
        if (Group == null)
            return;

        var group = new QueryGroup();
        Group.Filters.Add(group);

        QueryBuilder.Refresh();
    }

    protected void AddFilter()
    {
        if (Group == null)
            return;

        var group = new QueryFilter();
        Group.Filters.Add(group);

        QueryBuilder.Refresh();
    }
}
