using System.ComponentModel;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls;

public class DataContinuation : ComponentBase, IDisposable
{
    [CascadingParameter(Name = "PagerState")]
    protected DataPagerState PagerState { get; set; } = new();

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    public int PageSize { get; set; } = 25;

    [Parameter]
    public string FirstText { get; set; } = "« First";

    [Parameter]
    public string NextText { get; set; } = "Next >";

    [Parameter]
    public string PagerClass { get; set; } = "data-pager";

    [Parameter]
    public string ItemClass { get; set; } = "data-page-item";

    [Parameter]
    public string ButtonClass { get; set; } = "data-page-link";

    [Parameter]
    public string CurrentClass { get; set; } = "active";

    [Parameter]
    public string DisabledClass { get; set; } = "disabled";

    public void FirstPage()
    {
        PagerState.ContinuationToken = null;
    }

    public void NextPage()
    {
        if (PagerState.NextToken.HasValue())
            PagerState.ContinuationToken = PagerState.NextToken;
    }

    protected override void OnInitialized()
    {
        if (PagerState == null)
            throw new InvalidOperationException("DataSizer requires a cascading parameter PagerState.");

        // copy defaults to state
        if (PagerState.PageSize != PageSize)
            PagerState.Attach(1, PageSize);

        PagerState.PropertyChanged += OnStatePropertyChange;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "nav");
        builder.AddAttribute(1, "role", "navigation");
        builder.AddAttribute(2, "aria-label", "Pagination Navigation");
        builder.AddMultipleAttributes(3, AdditionalAttributes);

        builder.OpenElement(3, "ul");
        builder.AddAttribute(4, "role", "list");
        builder.AddAttribute(5, "class", PagerClass);

        var firstPageDisabledClass = PagerState.ContinuationToken.HasValue() ? null : DisabledClass;
        RenderPageLink(builder, FirstText, "Go to first page", FirstPage, firstPageDisabledClass);

        var disabledClass = PagerState.NextToken.HasValue() ? CurrentClass : DisabledClass;
        RenderPageLink(builder, NextText, "Go to next page", NextPage, disabledClass);

        builder.CloseElement(); // ul

        builder.CloseElement(); // nav

    }

    private void RenderPageLink(
        RenderTreeBuilder builder,
        string text,
        string title,
        Action action,
        string? disabledClass = null)
    {
        var enabled = string.IsNullOrEmpty(disabledClass);
        var isCurrent = disabledClass == CurrentClass;
        var isDisabled = disabledClass == DisabledClass;

        var itemClass = CssBuilder
            .Default(ItemClass)
            .AddClass(disabledClass, !enabled)
            .ToString();

        builder.OpenElement(5, "li");
        builder.AddAttribute(6, "class", itemClass);
        builder.AddAttribute(7, "aria-disabled", isDisabled);

        if (!isDisabled)
        {
            builder.OpenElement(9, "button");
            builder.AddAttribute(10, "class", ButtonClass);
            builder.AddAttribute(11, "type", "button");
            builder.AddAttribute(12, "title", title);
            builder.AddAttribute(14, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, action));
            builder.AddContent(15, text);
            builder.CloseElement(); // button
        }
        else
        {
            builder.OpenElement(16, "span");
            builder.AddAttribute(17, "class", ButtonClass);

            if (isCurrent)
                builder.AddAttribute(18, "aria-current", "page");

            if (isDisabled)
                builder.AddAttribute(19, "aria-hidden", true);

            builder.AddContent(20, text);
            builder.CloseElement(); // span
        }

        builder.CloseElement(); // li
    }

    private void OnStatePropertyChange(object? sender, PropertyChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        PagerState.PropertyChanged -= OnStatePropertyChange;
        GC.SuppressFinalize(this);
    }
}
