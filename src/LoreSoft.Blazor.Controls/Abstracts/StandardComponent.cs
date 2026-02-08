using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls.Abstracts;

/// <summary>
/// Base component class that provides standard functionality for Blazor components including
/// ID generation, CSS class and style building, element references, and disposable pattern implementation.
/// </summary>
public class StandardComponent : ComponentBase, IDisposable, IAsyncDisposable
{
    private Queue<Func<ValueTask>>? _afterRenderQueue;

    /// <summary>
    /// Gets or sets the unique identifier for the component element.
    /// If not specified, a random identifier will be automatically generated.
    /// </summary>
    [Parameter]
    public string? ElementId { get; set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied to the component element.
    /// This enables the capture of unmatched attributes from the component's usage.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }


    /// <summary>
    /// Gets the base CSS class(es) for the component element.
    /// Override this property in derived classes to provide default styling.
    /// </summary>
    protected virtual string? ElementClass { get; }

    /// <summary>
    /// Gets the base CSS style(s) for the component element.
    /// Override this property in derived classes to provide default inline styles.
    /// </summary>
    protected virtual string? ElementStyle { get; }

    /// <summary>
    /// Gets or sets the reference to the component's rendered element.
    /// This can be used to interact with the DOM element directly via JavaScript interop.
    /// </summary>
    protected ElementReference? Element { get; set; }


    /// <summary>
    /// Gets the computed element identifier used in rendering.
    /// This will either be the user-specified <see cref="ElementId"/> or an auto-generated random identifier.
    /// </summary>
    protected string? BoundElementId { get; private set; }

    /// <summary>
    /// Gets the computed CSS class string that combines <see cref="ElementClass"/>,
    /// additional attributes, and any derived class modifications.
    /// </summary>
    protected string? BoundClass { get; private set; }

    /// <summary>
    /// Gets the computed CSS style string that combines <see cref="ElementStyle"/>,
    /// additional attributes, and any derived class modifications.
    /// </summary>
    protected string? BoundStyle { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the component has been rendered at least once.
    /// </summary>
    protected bool Rendered { get; private set; }



    /// <summary>
    /// Called when component parameters are set or changed.
    /// Computes the element ID, CSS classes, and styles for the component.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ElementId != null && ElementId != BoundElementId)
            BoundElementId = ElementId;

        BoundElementId ??= Identifier.Random();

        BoundClass = ComputeClasses();
        BoundStyle = ComputeStyles();
    }

    /// <summary>
    /// Called after the component has been rendered.
    /// Processes any queued actions that were scheduled to run after rendering.
    /// </summary>
    /// <param name="firstRender">Indicates whether this is the first time the component has been rendered.</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Rendered = true;
        await ProcessRenderQueue();
        await base.OnAfterRenderAsync(firstRender);
    }


    /// <summary>
    /// Queues an action to be executed after the next render cycle.
    /// This is useful for operations that require the DOM to be updated before executing.
    /// </summary>
    /// <param name="action">The asynchronous action to execute after rendering.</param>
    /// <remarks>
    /// Actions are executed in the order they were queued during the <see cref="OnAfterRenderAsync"/> lifecycle method.
    /// </remarks>
    protected void ExecuteAfterRender(Func<ValueTask> action)
    {
        _afterRenderQueue ??= new();
        _afterRenderQueue.Enqueue(action);
    }

    /// <summary>
    /// Processes and executes all queued after-render actions.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    protected async ValueTask ProcessRenderQueue()
    {
        if (_afterRenderQueue == null || _afterRenderQueue.Count == 0)
            return;

        while (_afterRenderQueue.Count > 0)
        {
            var action = _afterRenderQueue.Dequeue();
            await action();
        }
    }


    private string? ComputeStyles()
    {
        return StyleBuilder.Pool.Use(builder =>
        {
            builder.AddStyle(ElementStyle).MergeStyle(AdditionalAttributes);

            // allow derived classes to modify styles
            ComputeAttributes(builder);

            return builder.ToString();
        });
    }

    /// <summary>
    /// Allows derived classes to add or modify CSS styles for the component.
    /// Override this method to add conditional or dynamic styles.
    /// </summary>
    /// <param name="styleBuilder">The <see cref="StyleBuilder"/> instance to modify.</param>
    /// <returns>The modified <see cref="StyleBuilder"/> instance.</returns>
    protected virtual StyleBuilder ComputeAttributes(StyleBuilder styleBuilder) => styleBuilder;


    private string? ComputeClasses()
    {
        return CssBuilder.Pool.Use(builder =>
        {
            builder.AddClass(ElementClass).MergeClass(AdditionalAttributes);

            // allow derived classes to modify classes
            ComputeClasses(builder);

            return builder.ToString();
        });
    }

    /// <summary>
    /// Allows derived classes to add or modify CSS classes for the component.
    /// Override this method to add conditional or dynamic classes.
    /// </summary>
    /// <param name="cssBuilder">The <see cref="CssBuilder"/> instance to modify.</param>
    /// <returns>The modified <see cref="CssBuilder"/> instance.</returns>
    protected virtual CssBuilder ComputeClasses(CssBuilder cssBuilder) => cssBuilder;


    #region Disposable Pattern
    /// <summary>
    /// Gets a value indicating whether this instance has been disposed.
    /// </summary>
    protected bool Disposed { get; private set; }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Disposed)
            return;

        await DisposeAsyncCore().ConfigureAwait(false);
        DisposeUnmanagedResources();

        Disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources;
    /// <c>false</c> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (Disposed)
            return;

        if (disposing)
            DisposeManagedResources();

        DisposeUnmanagedResources();

        Disposed = true;
    }

    /// <summary>
    /// Disposes managed resources asynchronously.
    /// Override this method in derived classes to perform async cleanup.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    protected virtual ValueTask DisposeAsyncCore()
    {
        // Default implementation: dispose managed resources synchronously
        DisposeManagedResources();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Override this method to dispose managed resources.
    /// </summary>
    protected virtual void DisposeManagedResources()
    {
        // Override in derived classes
    }

    /// <summary>
    /// Override this method to dispose unmanaged resources.
    /// </summary>
    protected virtual void DisposeUnmanagedResources()
    {
        // Override in derived classes
    }

    /// <summary>
    /// Throws an ObjectDisposedException if this instance has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the component has been disposed.</exception>
    protected void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(Disposed, this);
    #endregion
}
