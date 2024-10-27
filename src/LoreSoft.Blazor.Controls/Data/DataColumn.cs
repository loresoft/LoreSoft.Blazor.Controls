using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public class DataColumn<TItem> : ComponentBase
{
    private Func<TItem, object> _propertyAccessor;

    [CascadingParameter(Name = "Grid")]
    protected DataGrid<TItem> Grid { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> UnmatchedAttributes { get; set; }

    [Parameter]
    public Func<TItem, Dictionary<string, object>> CellAttributes { get; set; }

    [Parameter]
    public Expression<Func<TItem, object>> Property { get; set; }

    [Parameter]
    public string Title { get; set; }

    [Parameter]
    public string Width { get; set; }

    [Parameter]
    public string Format { get; set; }

    [Parameter]
    public string ClassName { get; set; }

    [Parameter]
    public Func<TItem, string> Style { get; set; }

    [Parameter]
    public string HeaderClass { get; set; }

    [Parameter]
    public string FooterClass { get; set; }


    [Parameter]
    public string ColumnClass { get; set; }

    [Parameter]
    public string ColumnStyle { get; set; }


    [Parameter]
    public bool Sortable { get; set; } = true;

    [Parameter]
    public int SortIndex { get; set; } = -1;

    [Parameter]
    public bool SortDescending { get; set; }


    [Parameter]
    public bool Filterable { get; set; } = true;


    [Parameter]
    public bool Groupable { get; set; } = true;

    [Parameter]
    public bool Grouping { get; set; }

    [Parameter]
    public RenderFragment<IGrouping<string, TItem>> GroupTemplate { get; set; }


    [Parameter]
    public bool Visible { get; set; } = true;

    [Parameter]
    public RenderFragment HeaderTemplate { get; set; }

    [Parameter]
    public RenderFragment<TItem> Template { get; set; }

    [Parameter]
    public RenderFragment<ICollection<TItem>> FooterTemplate { get; set; }

    [Parameter]
    public RenderFragment<QueryFilter> FilterTemplate { get; set; }

    public string PropertyName { get; set; }

    public string ColumnName { get; set; }

    public Type PropertyType { get; set; }

    internal int CurrentSortIndex { get; set; } = -1;

    internal bool CurrentSortDescending { get; set; }

    internal string CurrentColumnStyle { get; set; }

    protected override void OnInitialized()
    {
        if (Grid == null)
            throw new InvalidOperationException("DataColumn must be child of DataGrid");

        if (Property == null)
            throw new InvalidOperationException("DataColumn Property parameter is required");

        CurrentSortIndex = SortIndex;
        CurrentSortDescending = SortDescending;

        // register with parent grid
        Grid.AddColumn(this);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        CurrentColumnStyle = new StyleBuilder()
            .AddStyle(ColumnStyle)
            .AddStyle("width", Width, (v) => v.HasValue())
            .ToString();

        UpdateProperty();
    }

    internal string HeaderTitle()
    {
        if (!string.IsNullOrEmpty(Title))
            return Title;

        return PropertyName.ToTitle();
    }

    internal string CellValue(TItem data)
    {
        if (data == null || Property == null)
            return string.Empty;

        _propertyAccessor ??= Property.Compile();

        object value = null;

        try
        {
            value = _propertyAccessor.Invoke(data);
        }
        catch (NullReferenceException)
        {

        }

        if (value == null)
            return string.Empty;

        return string.IsNullOrEmpty(Format)
            ? value.ToString()
            : string.Format(CultureInfo.CurrentCulture, $"{{0:{Format}}}", value);
    }

    internal void UpdateSort(int index, bool descending)
    {
        CurrentSortIndex = index;
        CurrentSortDescending = descending;
    }

    private void UpdateProperty()
    {
        MemberInfo memberInfo = null;

        if (Property?.Body is MemberExpression memberExpression)
            memberInfo = memberExpression.Member;
        else if (Property?.Body is UnaryExpression { Operand: MemberExpression memberOperand })
            memberInfo = memberOperand.Member;

        if (memberInfo is PropertyInfo propertyInfo)
        {
            PropertyName = propertyInfo.Name;
            PropertyType = propertyInfo.PropertyType;
        }
        else if (memberInfo is FieldInfo fieldInfo)
        {
            PropertyName = fieldInfo.Name;
            PropertyType = fieldInfo.FieldType;
        }
        else
        {
            PropertyName = memberInfo.Name;
            PropertyType = typeof(object);
        }

        var columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>(true);
        ColumnName = columnAttribute != null ? columnAttribute.Name : PropertyName;
    }

    internal Dictionary<string, object> ComputeAttributes(TItem data)
    {
        var attributes = new Dictionary<string, object>();
        if (CellAttributes != null)
        {
            var computed = CellAttributes(data);
            if (computed != null)
                foreach (var attribute in computed)
                    attributes[attribute.Key] = attribute.Value;
        }

        if (UnmatchedAttributes != null)
            foreach (var attribute in UnmatchedAttributes)
                attributes[attribute.Key] = attribute.Value;

        return attributes;
    }
}
