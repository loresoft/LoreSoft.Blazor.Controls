using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public partial class QueryBuilderFilter<TItem>
{
    [CascadingParameter(Name = "QueryBuilder")]
    public required QueryBuilder<TItem> QueryBuilder { get; set; }

    [Parameter, EditorRequired]
    public QueryFilter Filter { get; set; } = new();

    [Parameter]
    public QueryGroup Parent { get; set; }


    protected QueryBuilderField<TItem> Field { get; set; }

    protected List<QueryBuilderField<TItem>> Fields => QueryBuilder.Fields;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        FieldChanged();
    }

    protected void FieldChanged()
    {
        Field = Fields.FirstOrDefault(f => f.Column == Filter.Field);
        QueryBuilder.Refresh();
    }

    protected void OperatorChanged()
    {
        if (Filter.Operator == QueryOperators.IsNull || Filter.Operator == QueryOperators.IsNotNull)
            Filter.Value = null;

        QueryBuilder.Refresh();
    }

    protected bool ShowValueInput
        => Filter.Operator is not QueryOperators.IsNull or QueryOperators.IsNotNull;

    protected void DeleteFilter()
    {
        if (Parent == null)
            return;

        Parent.Filters.Remove(Filter);

        QueryBuilder.Refresh();
    }

    protected string GetValue()
    {
        return Binding.Format(Filter.Value);
    }

    protected void SetValue(ChangeEventArgs args)
    {
        Filter.Value = Binding.Convert(args.Value, Field.Type);
        QueryBuilder.Refresh();
    }
}
