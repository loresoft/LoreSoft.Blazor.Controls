using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls
{
    public partial class Skeleton : ComponentBase
    {
        [Parameter]
        public string Width { set; get; }

        [Parameter]
        public string Height { set; get; }

        [Parameter]
        public SkeletonType Type { set; get; } = SkeletonType.Text;

        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();

        protected string Style { get; set; } = string.Empty;

        protected override void OnInitialized()
        {

            Style = string.Empty;

            if (!string.IsNullOrEmpty(Width))
            {
                Style += $"width:{Width};";
            }

            if (!string.IsNullOrEmpty(Height))
            {
                Style += $"height:{Height};";
            }
        }
    }


    public enum SkeletonType
    {
        [Description("text")]
        Text,
        [Description("circle")]
        Circle,
        [Description("rectangle")]
        Rectangle
    }
}
