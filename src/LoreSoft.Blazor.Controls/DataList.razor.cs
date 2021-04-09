using System.Collections.Generic;
using System.Text;
using LoreSoft.Blazor.Controls.Utilities;
using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls
{
    public partial class DataList<TItem> : DataComponentBase<TItem>
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> Attributes { get; set; }

        [Parameter]
        public RenderFragment<TItem> RowTemplate { get; set; }

        [Parameter]
        public RenderFragment HeaderTemplate { get; set; }

        [Parameter]
        public RenderFragment FooterTemplate { get; set; }


        protected string ClassName { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            ClassName = new CssBuilder("data-list")
                        .MergeClass(Attributes)
                        .ToString();
        }
    }
}
