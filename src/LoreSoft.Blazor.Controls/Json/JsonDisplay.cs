#nullable enable

using System.Text.Json;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component for displaying JSON data in a tabular format.
/// Supports both raw JSON strings and <see cref="JsonElement"/> values.
/// </summary>
public class JsonDisplay : ComponentBase
{
    /// <summary>
    /// Gets or sets the JSON string to display.
    /// If set, this will be parsed and rendered.
    /// </summary>
    [Parameter]
    public string? Json { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="JsonElement"/> to display.
    /// Used if <see cref="Json"/> is not set.
    /// </summary>
    [Parameter]
    public JsonElement? JsonElement { get; set; }

    /// <summary>
    /// Additional attributes to be applied to the root table element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? Attributes { get; set; }

    /// <summary>
    /// Builds the render tree for the JSON display component.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the component's render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!string.IsNullOrEmpty(Json))
        {
            var document = JsonDocument.Parse(Json);
            AppendValue(builder, document.RootElement);
        }
        else if (JsonElement.HasValue)
        {
            AppendValue(builder, JsonElement.Value);
        }
    }

    /// <summary>
    /// Appends the value of a <see cref="JsonElement"/> to the render tree.
    /// Handles objects, arrays, and primitive values.
    /// </summary>
    /// <param name="builder">The render tree builder.</param>
    /// <param name="jsonElement">The JSON element to render.</param>
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

    /// <summary>
    /// Appends a JSON object as a table to the render tree.
    /// Each property is rendered as a row.
    /// </summary>
    /// <param name="builder">The render tree builder.</param>
    /// <param name="jsonElement">The JSON object element to render.</param>
    private void AppendObject(RenderTreeBuilder builder, JsonElement jsonElement)
    {
        builder.OpenElement(0, "table");
        builder.AddAttribute(1, "class", "json-object");
        builder.AddMultipleAttributes(2, Attributes);
        builder.SetKey(jsonElement);

        foreach (var jsonProperty in jsonElement.EnumerateObject())
        {
            builder.OpenElement(3, "tr");
            builder.SetKey(jsonProperty.Name);

            builder.OpenElement(4, "th");
            builder.AddAttribute(5, "class", "json-name");
            builder.AddContent(6, jsonProperty.Name);
            builder.CloseElement(); // th

            builder.OpenElement(7, "td");
            builder.AddAttribute(8, "class", "json-value");

            AppendValue(builder, jsonProperty.Value);

            builder.CloseElement(); // td

            builder.CloseElement(); // tr
        }

        builder.CloseElement(); // table
    }
}

