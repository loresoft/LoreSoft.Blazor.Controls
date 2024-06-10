using System.Globalization;
using System.Linq.Expressions;

using LoreSoft.Blazor.Controls.Extensions;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls;

public class DataColumn<TItem> : ComponentBase
{
    private Func<TItem, object> _propertyAccessor;
    private string _propertyName;

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
    public bool Sortable { get; set; } = true;

    [Parameter]
    public int SortIndex { get; set; } = -1;

    [Parameter]
    public bool SortDescending { get; set; }


    [Parameter]
    public bool Filterable { get; set; } = true;


    [Parameter]
    public bool Visible { get; set; } = true;

    public string Name => PropertyName();


    [Parameter]
    public RenderFragment HeaderTemplate { get; set; }

    [Parameter]
    public RenderFragment<TItem> Template { get; set; }

    [Parameter]
    public RenderFragment FooterTemplate { get; set; }

    [Parameter]
    public RenderFragment<QueryFilter> FilterTemplate { get; set; }


    internal int CurrentSortIndex { get; set; } = -1;

    internal bool CurrentSortDescending { get; set; }


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

    internal string HeaderTitle()
    {
        if (!string.IsNullOrEmpty(Title))
            return Title;

        var name = PropertyName();
        return name.ToTitle();
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

    private string PropertyName()
    {
        if (Property == null)
            return string.Empty;

        if (!string.IsNullOrEmpty(_propertyName))
            return _propertyName;

        _propertyName = Property?.Body switch
        {
            MemberExpression memberExpression => memberExpression.Member.Name,
            UnaryExpression { Operand: MemberExpression memberOperand } => memberOperand.Member.Name,
            _ => string.Empty
        };

        return _propertyName;
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
