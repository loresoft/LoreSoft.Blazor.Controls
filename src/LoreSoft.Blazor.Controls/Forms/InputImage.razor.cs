#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Diagnostics.CodeAnalysis;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// An input component for uploading and displaying images as data URLs.
/// </summary>
public partial class InputImage : InputBase<string>
{
    /// <summary>
    /// The default placeholder image (SVG) shown when no image is selected.
    /// </summary>
    private readonly string _placeHolder = "data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA1Mi41IDQ3LjI1IiB3aWR0aD0iNTIuNXB4IiBoZWlnaHQ9IjQ3LjI1cHgiPjxwYXRoIGZpbGwtcnVsZT0iZXZlbm9kZCIgY2xpcC1ydWxlPSJldmVub2RkIiBkPSJNIDAgMi42MDcgQyAwLjAxIDEuMTcyIDEuMTcgMC4wMTEgMi42MDQgMCBMIDQ5Ljg5NiAwIEMgNTEuMzM0IDAgNTIuNSAxLjE2OCA1Mi41IDIuNjA3IEwgNTIuNSA0NC42NDMgQyA1Mi40OSA0Ni4wNzggNTEuMzMgNDcuMjM5IDQ5Ljg5NiA0Ny4yNSBMIDIuNjA4IDQ3LjI1IEMgMS4xNjUgNDcuMjQ5IDAgNDYuMDgyIDAgNDQuNjQzIEwgMCAyLjYwNyBaIE0gNDcuMjUgNS4yNSBMIDUuMjUgNS4yNSBMIDUuMjUgNDIgTCAyOS42NDIgMTcuNjAzIEMgMzAuNjY3IDE2LjU3OSAzMi4zMjggMTYuNTc5IDMzLjM1NCAxNy42MDMgTCA0Ny4yNSAzMS41MjYgTCA0Ny4yNSA1LjI1IFogTSAxMC41IDE1Ljc1IEMgMTAuNSAxOC42NDkgMTIuODUgMjEgMTUuNzUgMjEgQyAxOC42NDkgMjEgMjEgMTguNjQ5IDIxIDE1Ljc1IEMgMjEgMTIuODUgMTguNjQ5IDEwLjUgMTUuNzUgMTAuNSBDIDEyLjg1IDEwLjUgMTAuNSAxMi44NSAxMC41IDE1Ljc1IFoiIGZpbGw9IiM2ODc3ODciIGlkPSJvYmplY3QtMCIvPjwvc3ZnPg==\r\n";

    /// <summary>
    /// Gets or sets the maximum allowed file size in bytes. Default is 5 MB.
    /// </summary>
    [Parameter]
    public long MaxFileSize { get; set; } = 1024 * 1024 * 5; // 5 MB

    /// <summary>
    /// Gets or sets the display height for the image in pixels.
    /// </summary>
    [Parameter]
    public int? DisplayHeight { get; set; }

    /// <summary>
    /// Gets or sets the display width for the image in pixels.
    /// </summary>
    [Parameter]
    public int? DisplayWidth { get; set; }

    /// <summary>
    /// Gets or sets the accepted file types for the input. Default is "image/*".
    /// </summary>
    [Parameter]
    public string? Accept { get; set; } = "image/*";

    /// <summary>
    /// Gets or sets a value indicating whether the input is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets a unique identifier for the input element.
    /// </summary>
    protected string InputIdentifier { get; } = Identifier.Random();

    /// <summary>
    /// Gets the image URL to display. Returns the placeholder if no image is selected.
    /// </summary>
    protected string? ImageUrl
        => string.IsNullOrWhiteSpace(CurrentValue) ? _placeHolder : CurrentValue;

    /// <summary>
    /// Gets the CSS style string for the image element.
    /// </summary>
    protected string? ImageStyle { get; set; }

    /// <summary>
    /// Sets the image style based on the provided height and width parameters.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Set the style for the image based on the provided height and width
        ImageStyle = StyleBuilder.Pool.Use(builder =>
        {
            return builder
                .AddStyle("height", $"{DisplayHeight}px", DisplayHeight.HasValue)
                .AddStyle("width", $"{DisplayWidth}px", DisplayWidth.HasValue)
                .ToString();
        });
    }

    /// <summary>
    /// Parses the input value from a string.
    /// </summary>
    /// <param name="value">The input string value.</param>
    /// <param name="result">The parsed result.</param>
    /// <param name="validationErrorMessage">The validation error message, if any.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out string result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value ?? string.Empty;
        validationErrorMessage = null;
        return true;
    }

    /// <summary>
    /// Loads the selected file and converts it to a data URL.
    /// </summary>
    /// <param name="e">The file change event arguments.</param>
    protected async Task LoadFiles(InputFileChangeEventArgs e)
        => CurrentValue = await ConvertToDataUrl(e.File);

    /// <summary>
    /// Clears the currently selected image.
    /// </summary>
    protected void ClearImage()
        => CurrentValue = string.Empty;

    /// <summary>
    /// Converts the specified browser file to a data URL.
    /// </summary>
    /// <param name="file">The browser file to convert.</param>
    /// <returns>A data URL string representing the image.</returns>
    private async Task<string> ConvertToDataUrl(IBrowserFile file)
    {
        if (file == null)
            return string.Empty;

        await using var stream = file.OpenReadStream(MaxFileSize);
        await using var memoryStream = new MemoryStream();

        await stream.CopyToAsync(memoryStream);

        var base64 = Convert.ToBase64String(memoryStream.ToArray());

        return $"data:{file.ContentType};base64,{base64}";
    }
}
