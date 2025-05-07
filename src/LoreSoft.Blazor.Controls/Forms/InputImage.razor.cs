#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Diagnostics.CodeAnalysis;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LoreSoft.Blazor.Controls;

public partial class InputImage : InputBase<string>
{
    private readonly string _placeHolder = "data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA1Mi41IDQ3LjI1IiB3aWR0aD0iNTIuNXB4IiBoZWlnaHQ9IjQ3LjI1cHgiPjxwYXRoIGZpbGwtcnVsZT0iZXZlbm9kZCIgY2xpcC1ydWxlPSJldmVub2RkIiBkPSJNIDAgMi42MDcgQyAwLjAxIDEuMTcyIDEuMTcgMC4wMTEgMi42MDQgMCBMIDQ5Ljg5NiAwIEMgNTEuMzM0IDAgNTIuNSAxLjE2OCA1Mi41IDIuNjA3IEwgNTIuNSA0NC42NDMgQyA1Mi40OSA0Ni4wNzggNTEuMzMgNDcuMjM5IDQ5Ljg5NiA0Ny4yNSBMIDIuNjA0IDQ3LjI1IEMgMS4xNjUgNDcuMjQ5IDAgNDYuMDgyIDAgNDQuNjQzIEwgMCAyLjYwNyBaIE0gNDcuMjUgNS4yNSBMIDUuMjUgNS4yNSBMIDUuMjUgNDIgTCAyOS42NDIgMTcuNjAzIEMgMzAuNjY3IDE2LjU3OSAzMi4zMjggMTYuNTc5IDMzLjM1NCAxNy42MDMgTCA0Ny4yNSAzMS41MjYgTCA0Ny4yNSA1LjI1IFogTSAxMC41IDE1Ljc1IEMgMTAuNSAxOC42NDkgMTIuODUgMjEgMTUuNzUgMjEgQyAxOC42NDkgMjEgMjEgMTguNjQ5IDIxIDE1Ljc1IEMgMjEgMTIuODUgMTguNjQ5IDEwLjUgMTUuNzUgMTAuNSBDIDEyLjg1IDEwLjUgMTAuNSAxMi44NSAxMC41IDE1Ljc1IFoiIGZpbGw9IiM2ODc3ODciIGlkPSJvYmplY3QtMCIvPjwvc3ZnPg==\r\n";

    [Parameter]
    public long MaxFileSize { get; set; } = 1024 * 1024 * 5; // 5 MB

    [Parameter]
    public int? DisplayHeight { get; set; }

    [Parameter]
    public int? DisplayWidth { get; set; }

    [Parameter]
    public string? Accept { get; set; } = "image/*";

    [Parameter]
    public bool Disabled { get; set; }

    protected string InputIdentifier { get; } = Guid.NewGuid().ToString("N");

    protected string? ImageUrl
        => string.IsNullOrWhiteSpace(CurrentValue) ? _placeHolder : CurrentValue;

    protected string? ImageStyle { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Set the style for the image based on the provided height and width
        ImageStyle = StyleBuilder.Empty()
            .AddStyle("height", $"{DisplayHeight}px", DisplayHeight.HasValue)
            .AddStyle("width", $"{DisplayWidth}px", DisplayWidth.HasValue)
            .ToString();
    }

    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out string result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value ?? string.Empty;
        validationErrorMessage = null;
        return true;
    }

    protected async Task LoadFiles(InputFileChangeEventArgs e)
        => CurrentValue = await ConvertToDataUrl(e.File);

    protected void ClearImage()
        => CurrentValue = string.Empty;


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
