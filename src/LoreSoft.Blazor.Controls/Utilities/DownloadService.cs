using System.Text;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides methods for downloading files in applications using JavaScript interop.
/// </summary>
public class DownloadService(IJSRuntime javaScript)
    : JavaScriptModule(javaScript, ModulePath)
{
    private const string ModulePath = "./_content/LoreSoft.Blazor.Controls/js/download.js";

    /// <summary>
    /// Downloads a file from a provided <see cref="Stream"/> to the user's device.
    /// </summary>
    /// <param name="stream">The stream containing the file data.</param>
    /// <param name="fileName">The name of the file to be downloaded. Optional.</param>
    /// <param name="mimeType">The MIME type of the file. Optional.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stream"/> is null.</exception>
    public async Task DownloadFileStream(
        Stream stream,
        string? fileName = null,
        string? mimeType = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var streamReference = new DotNetStreamReference(stream, true);
        await InvokeVoidAsync("downloadFileStream", streamReference, fileName, mimeType);
    }

    /// <summary>
    /// Downloads a text file to the user's device.
    /// </summary>
    /// <param name="text">The text content to be downloaded.</param>
    /// <param name="fileName">The name of the file to be downloaded. Optional.</param>
    /// <param name="mimeType">The MIME type of the file. Optional.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null.</exception>
    public async Task DownloadFileText(
        string text,
        string? fileName = null,
        string? mimeType = null)
    {
        ArgumentNullException.ThrowIfNull(text);

        var bytes = Encoding.UTF8.GetBytes(text);
        await using var stream = new MemoryStream(bytes);

        // need to reset stream position
        stream.Seek(0, SeekOrigin.Begin);

        await DownloadFileStream(stream, fileName, mimeType);
    }

    /// <summary>
    /// Triggers a file download from a specified URL.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="fileName">The name of the file to be downloaded. Optional.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="url"/> is null.</exception>
    public async Task TriggerFileDownload(
        string url,
        string? fileName = null)
    {
        ArgumentNullException.ThrowIfNull(url);

        await InvokeVoidAsync("triggerFileDownload", url, fileName);
    }
}
