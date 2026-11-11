using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Helper for loading any JavaScript (ES6) module and calling its exports
/// </summary>
public class JavaScriptModule : IAsyncDisposable
{
    private readonly IJSRuntime _javaScript;
    private readonly string _moduleUrl;
    private Task<IJSObjectReference>? _moduleTask;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaScriptModule"/> class.
    /// </summary>
    /// <param name="javaScript">The JavaScript runtime instance used to import the module.</param>
    /// <param name="moduleUrl">The URL of the JavaScript module to import.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="javaScript"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="moduleUrl"/> is null or whitespace.</exception>
    public JavaScriptModule(IJSRuntime javaScript, string moduleUrl)
    {
        ArgumentNullException.ThrowIfNull(javaScript);
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleUrl);

        _javaScript = javaScript;
        _moduleUrl = moduleUrl;
    }

    /// <summary>
    /// Invokes the specified JavaScript function asynchronously without returning a value.
    /// </summary>
    /// <param name="identifier">The identifier of the JavaScript function to invoke.</param>
    /// <param name="args">Optional arguments to pass to the JavaScript function.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// If the JavaScript runtime has disconnected (for example, the client closed the browser), the call is ignored.
    /// </remarks>
    public async ValueTask InvokeVoidAsync(string identifier, params object?[]? args)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync(identifier, args);
        }
        catch (JSDisconnectedException)
        {
            // the browser is gone, there is nothing to invoke
        }
    }

    /// <summary>
    /// Invokes the specified JavaScript function asynchronously and returns a value.
    /// </summary>
    /// <typeparam name="T">The type of the return value.</typeparam>
    /// <param name="identifier">The identifier of the JavaScript function to invoke.</param>
    /// <param name="args">Optional arguments to pass to the JavaScript function.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation, containing the result of the JavaScript function.
    /// Returns the default value of <typeparamref name="T"/> if the JavaScript runtime has disconnected.
    /// </returns>
    public async ValueTask<T> InvokeAsync<T>(string identifier, params object?[]? args)
    {
        try
        {
            var module = await GetModuleAsync();
            return await module.InvokeAsync<T>(identifier, args);
        }
        catch (JSDisconnectedException)
        {
            // the browser is gone, there is no result to return
            return default!;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeCoreAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs the core disposal logic for the JavaScript module.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    /// <remarks>
    /// This method safely disposes the JavaScript module reference while handling common exceptions
    /// that may occur when the JavaScript runtime is no longer available, such as when the client disconnects.
    /// </remarks>
    protected virtual async ValueTask DisposeCoreAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        var moduleTask = _moduleTask;
        if (moduleTask is null || moduleTask.IsFaulted || moduleTask.IsCanceled)
            return;

        try
        {
            var module = await moduleTask;
            await module.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or OperationCanceledException or InvalidOperationException)
        {
            // The JSRuntime side may routinely be gone already if the reason we're disposing is that
            // the client disconnected. This is not an error.
        }
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        try
        {
            _moduleTask ??= _javaScript.InvokeAsync<IJSObjectReference>("import", _moduleUrl).AsTask();
            return await _moduleTask;
        }
        catch
        {
            // don't cache a failed import (e.g. called during prerendering) so the next call can retry
            _moduleTask = null;
            throw;
        }
    }
}
