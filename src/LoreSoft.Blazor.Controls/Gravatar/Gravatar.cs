using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

public class Gravatar : ComponentBase, IDisposable
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; }

    [Parameter]
    public string Email { get; set; }

    [Parameter]
    public GravatarMode Mode { get; set; } = GravatarMode.Mm;

    [Parameter]
    public GravatarRating Rating { get; set; } = GravatarRating.g;

    [Parameter]
    public int Size { get; set; } = 50;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var hash = GetHash(Email);
        var mode = GetModeValue(Mode);
        var url = $"https://gravatar.com/avatar/{hash}?s={Size}&d={mode}&r={Rating}";

        builder.OpenElement(0, "img");
        builder.AddAttribute(1, "src", url);
        builder.AddAttribute(2, "alt", Email);
        builder.AddMultipleAttributes(3, Attributes);
        builder.CloseElement(); // img
    }

    public void Dispose()
    {
    }

    private static string GetHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var text = value
            .Trim()
            .ToLower();

        using var md5 = new MD5Managed();
        byte[] buffer = Encoding.ASCII.GetBytes(text);
        byte[] bytes = md5.ComputeHash(buffer);

        var hash = BitConverter
            .ToString(bytes)
            .Replace("-", "")
            .ToLower();

        return hash;
    }

    private static string GetModeValue(GravatarMode mode)
    {
        return mode == GravatarMode.NotFound ? "404" : mode.ToString().ToLower();
    }

}
