// Ignore Spelling: Gravatar

using System.Text;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// A component for displaying a Gravatar image based on an email address.
/// Supports Gravatar modes, ratings, and custom sizing.
/// </summary>
public class Gravatar : ComponentBase, IDisposable
{
    /// <summary>
    /// Additional attributes to be applied to the <c>img</c> element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// The email address used to generate the Gravatar image.
    /// </summary>
    [Parameter, EditorRequired]
    public required string Email { get; set; }

    /// <summary>
    /// The Gravatar mode to use when no image is found or for default avatars.
    /// </summary>
    [Parameter]
    public GravatarMode Mode { get; set; } = GravatarMode.Mm;

    /// <summary>
    /// The Gravatar rating to restrict which images are shown.
    /// </summary>
    [Parameter]
    public GravatarRating Rating { get; set; } = GravatarRating.g;

    /// <summary>
    /// The size (in pixels) of the Gravatar image.
    /// </summary>
    [Parameter]
    public int Size { get; set; } = 50;

    /// <summary>
    /// Builds the render tree for the Gravatar image.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the component's render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var hash = GetHash(Email);
        var mode = GetModeValue(Mode);
        var url = $"https://gravatar.com/avatar/{hash}?s={Size}&d={mode}&r={Rating}";

        builder.OpenElement(0, "img");
        builder.AddAttribute(1, "src", url);
        builder.AddAttribute(2, "alt", Email);
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.CloseElement(); // img
    }

    /// <summary>
    /// Disposes resources used by the component.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Computes the MD5 hash of the email address for Gravatar.
    /// </summary>
    /// <param name="value">The email address to hash.</param>
    /// <returns>The MD5 hash string in lowercase hexadecimal format.</returns>
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

    /// <summary>
    /// Gets the string value for the Gravatar mode.
    /// </summary>
    /// <param name="mode">The Gravatar mode.</param>
    /// <returns>The string value for the mode, or "404" for <see cref="GravatarMode.NotFound"/>.</returns>
    private static string GetModeValue(GravatarMode mode)
    {
        return mode == GravatarMode.NotFound ? "404" : mode.ToString().ToLower();
    }
}
