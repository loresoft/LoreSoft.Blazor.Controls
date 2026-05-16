#nullable enable

using System.Text.Json;

using LoreSoft.Blazor.Controls.Extensions;

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
    /// Gets or sets an optional template used to render primitive JSON values.
    /// The template receives a <see cref="JsonValueContext"/> containing the
    /// <see cref="JsonElement"/> and its JSONPath location within the document.
    /// When <c>null</c>, the value is rendered using the default content builder.
    /// </summary>
    [Parameter]
    public RenderFragment<JsonValueContext>? ValueTemplate { get; set; }

    /// <summary>
    /// Additional attributes to be applied to the root table element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? Attributes { get; set; }

    // only compute JSONPath if a ValueTemplate is provided, since that's the only scenario where it's needed.
    private bool ComputePath => ValueTemplate is not null;

    /// <summary>
    /// Builds the render tree for the JSON display component.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the component's render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var path = ComputePath ? "$" : null;

        if (!string.IsNullOrEmpty(Json))
        {
            var document = JsonDocument.Parse(Json);
            AppendValue(builder, document.RootElement, path);
        }
        else if (JsonElement.HasValue)
        {
            AppendValue(builder, JsonElement.Value, path);
        }
    }

    /// <summary>
    /// Appends the value of a <see cref="JsonElement"/> to the render tree.
    /// Handles objects, arrays, and primitive values.
    /// </summary>
    /// <param name="builder">The render tree builder.</param>
    /// <param name="jsonElement">The JSON element to render.</param>
    /// <param name="path">The JSONPath location of <paramref name="jsonElement"/> within the document.</param>
    private void AppendValue(RenderTreeBuilder builder, JsonElement jsonElement, string? path)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
                AppendObject(builder, jsonElement, path);
                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var arrayElement in jsonElement.EnumerateArray())
                {
                    var indexPath = ComputePath && path.HasValue() ? $"{path}[{index}]" : null;
                    AppendValue(builder, arrayElement, indexPath);
                    index++;
                }
                break;
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                if (ValueTemplate is not null)
                    builder.AddContent(0, ValueTemplate(new JsonValueContext(jsonElement, path)));
                else
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
    /// <param name="path">The JSONPath location of <paramref name="jsonElement"/> within the document.</param>
    private void AppendObject(RenderTreeBuilder builder, JsonElement jsonElement, string? path)
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

            var propertyPath = ComputePath && path.HasValue() ? BuildPropertyPath(path, jsonProperty.Name) : null;
            AppendValue(builder, jsonProperty.Value, propertyPath);

            builder.CloseElement(); // td

            builder.CloseElement(); // tr
        }

        builder.CloseElement(); // table
    }

    private static string BuildPropertyPath(string parentPath, string propertyName)
    {
        // Use dot notation for identifier-safe names; otherwise use bracket notation with escaping.
        if (IsIdentifier(propertyName))
            return $"{parentPath}.{propertyName}";

        var escaped = propertyName.Replace("\\", "\\\\").Replace("'", "\\'");
        return $"{parentPath}['{escaped}']";
    }

    private static bool IsIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        if (!char.IsLetter(name[0]) && name[0] != '_')
            return false;

        for (var i = 1; i < name.Length; i++)
        {
            if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                return false;
        }

        return true;
    }
}

/// <summary>
/// Context passed to <see cref="JsonDisplay.ValueTemplate"/> when rendering a primitive JSON value.
/// </summary>
/// <param name="Element">The JSON element being rendered.</param>
/// <param name="Path">The JSONPath location of <paramref name="Element"/> within the document.</param>
public readonly record struct JsonValueContext(JsonElement Element, string? Path);

