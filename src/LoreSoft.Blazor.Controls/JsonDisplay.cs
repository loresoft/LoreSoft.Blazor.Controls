using System.Text.Json;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class JsonDisplay : ComponentBase
{
    [Parameter]
    public string Json { get; set; }

    [Parameter]
    public JsonElement JsonElement { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? Attributes { get; set; }


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!string.IsNullOrEmpty(Json))
        {
            var document = JsonDocument.Parse(Json);
            AppendValue(builder, document.RootElement);
        }
        else
        {
            AppendValue(builder, JsonElement);
        }
    }

    private void AppendValue(RenderTreeBuilder builder, JsonElement jsonElement)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
                AppendObject(builder, jsonElement);
                break;
            case JsonValueKind.Array:
                foreach (var arrayElement in jsonElement.EnumerateArray())
                    AppendValue(builder, arrayElement);

                break;
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                builder.AddContent(0, jsonElement.ToString());
                break;
            default:
                builder.AddContent(1, string.Empty);
                break;
        }
    }

    private void AppendObject(RenderTreeBuilder builder, JsonElement jsonElement)
    {
        builder.OpenElement(3, "table");
        builder.AddAttribute(4, "class", "json-object");
        builder.AddMultipleAttributes(5, Attributes);

        foreach (var jsonProperty in jsonElement.EnumerateObject())
        {
            builder.OpenElement(6, "tr");
            builder.SetKey(jsonProperty.Name);

            builder.OpenElement(7, "th");
            builder.AddAttribute(8, "class", "json-name");
            builder.AddContent(9, jsonProperty.Name);
            builder.CloseElement(); // th

            builder.OpenElement(10, "td");
            builder.AddAttribute(11, "class", "json-value");

            AppendValue(builder, jsonProperty.Value);

            builder.CloseElement(); // td

            builder.CloseElement(); // tr
        }

        builder.CloseElement(); // table
    }
}

