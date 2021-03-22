using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls
{
    public class DataSizer : ComponentBase
    {
        [CascadingParameter(Name = "PagerState")]
        protected DataPagerState PagerState { get; set; } = new();

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes { get; set; }

        [Parameter]
        public int[] PageSizeOptions { get; set; } = { 10, 25, 50, 100 };

        [Parameter]
        public bool IncludeAllOption { get; set; }

        [Parameter]
        public string DescriptionLabel { get; set; } = "Items per page";


        protected override void OnInitialized()
        {
            PagerState.PropertyChanged += OnStatePropertyChange;

        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "data-page-size-options");
            builder.AddMultipleAttributes(2, Attributes);

            builder.OpenElement(3, "select");
            builder.AddAttribute(4, "value", PagerState.PageSize);
            builder.AddAttribute(5, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, e => SetPageSize(Convert.ToInt32(e.Value ?? 0))));

            foreach (var pageSizeOption in PageSizeOptions)
            {
                builder.OpenElement(6, "option");
                builder.AddAttribute(7, "value", pageSizeOption);
                builder.SetKey(pageSizeOption);
                builder.AddContent(8, pageSizeOption);
                builder.CloseElement(); // option
            }

            if (IncludeAllOption)
            {
                builder.OpenElement(9, "option");
                builder.AddAttribute(10, "value", 0);
                builder.AddContent(11, "All");
                builder.CloseElement(); // option
            }

            builder.CloseElement(); // select

            if (!string.IsNullOrEmpty(DescriptionLabel))
            {
                builder.OpenElement(12, "span");
                builder.AddContent(13, DescriptionLabel);
                builder.CloseElement(); // span
            }

            builder.CloseElement(); // div
        }


        private void SetPageSize(int pageSize)
        {
            PagerState.PageSize = pageSize;
        }

        private void OnStatePropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataPagerState.PageSize))
                StateHasChanged();
        }
    }
}
