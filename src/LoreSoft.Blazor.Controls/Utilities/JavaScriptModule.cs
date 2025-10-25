using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Helper for loading any JavaScript (ES6) module and calling its exports
/// </summary>
public abstract class JavaScriptModule : IAsyncDisposable
{
    private readonly IJSRuntime _javaScript;
    private readonly Lazy<ValueTask<IJSObjectReference>> _moduleTask;
    private bool _isDisposed;

    protected JavaScriptModule(IJSRuntime javaScript, string moduleUrl)
    {
        ArgumentNullException.ThrowIfNull(javaScript);
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleUrl);

        _javaScript = javaScript;
        _moduleTask = new(() => _javaScript.InvokeAsync<IJSObjectReference>("import", moduleUrl));
    }

    protected async ValueTask InvokeVoidAsync(string identifier, params object?[]? args)
        => await (await _moduleTask.Value).InvokeVoidAsync(identifier, args);

    protected async ValueTask<T> InvokeAsync<T>(string identifier, params object?[]? args)
        => await (await _moduleTask.Value).InvokeAsync<T>(identifier, args);


    public async ValueTask DisposeAsync()
    {
        await DisposeCoreAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeCoreAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        if (!_moduleTask.IsValueCreated || _moduleTask.Value.IsFaulted)
            return;

        try
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync().ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            // This can be called too early when using pre-rendering
        }
        catch (Exception ex) when (ex is JSDisconnectedException || ex is OperationCanceledException)
        {
            // The JSRuntime side may routinely be gone already if the reason we're disposing is that
            // the client disconnected. This is not an error.
        }
    }
}
