using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Component that renders and manages a modal dialog using the HTML <c>&lt;dialog&gt;</c> element with JavaScript interop.
/// </summary>
/// <remarks>
/// This component handles the lifecycle of a single modal dialog, including opening, closing, and cleanup.
/// It uses JavaScript interop to manage the native dialog element and subscribes to modal close messages.
/// </remarks>
public partial class ModalDialog : ComponentBase, IAsyncDisposable
{
    // Special return value indicating the dialog was closed programmatically, used to prevent duplicate close notifications.
    private const string ProgrammaticCloseValue = "--closed--";

    private readonly Messenger _messenger;

    private IJSObjectReference? _module;
    private IJSObjectReference? _dialog;
    private DotNetObjectReference<ModalDialog>? _instance;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalDialog"/> class.
    /// </summary>
    /// <param name="messenger">The messenger service used to subscribe to modal close messages.</param>
    public ModalDialog(Messenger messenger)
    {
        _messenger = messenger;
        _messenger.Subscribe<ModalClose>(this, HandleModalClose);
    }

    /// <summary>
    /// Gets or sets the JavaScript runtime for interop operations.
    /// </summary>
    /// <value>The injected <see cref="IJSRuntime"/> instance.</value>
    [Inject]
    public IJSRuntime JavaScript { get; set; } = null!;

    /// <summary>
    /// Gets or sets the modal reference containing the component type, parameters, and modal ID.
    /// </summary>
    /// <value>The <see cref="ModalReference"/> for this dialog.</value>
    [Parameter]
    public ModalReference Modal { get; set; } = null!;

    /// <summary>
    /// Gets or sets the reference to the HTML dialog element.
    /// </summary>
    /// <value>The element reference for the dialog DOM element.</value>
    protected ElementReference Element { get; set; }

    /// <summary>
    /// Gets the CSS class names to apply to the dialog container.
    /// </summary>
    /// <value>The combined CSS class string. Defaults to "dialog-container".</value>
    protected string ClassName { get; private set; } = "dialog-container";

    /// <summary>
    /// Gets the inline styles to apply to the dialog container.
    /// </summary>
    /// <value>The combined inline style string.</value>
    protected string Style { get; private set; } = string.Empty;

    /// <summary>
    /// JavaScript-invokable method called when the dialog is closed via the native dialog close event.
    /// </summary>
    /// <param name="returnValue">The return value from the dialog. A value of "--closed--" indicates programmatic closure.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// When the dialog is closed by user interaction (e.g., pressing ESC or clicking outside), this method
    /// triggers a cancel result. Programmatic closes are detected by the special "--closed--" return value
    /// to prevent duplicate close notifications.
    /// </remarks>
    [JSInvokable]
    public async Task OnDialogClosed(string? returnValue)
    {
        if (_disposed)
            return;

        // If the dialog was closed programmatically, do not send another close message
        if (returnValue == ProgrammaticCloseValue)
            return;

        await Modal.CloseAsync(ModalResult.Cancel());
    }

    /// <summary>
    /// Called when component parameters are set. Builds the CSS classes and inline styles from modal parameters.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ClassName = new CssBuilder("dialog-container")
            .MergeClass(Modal.Parameters, false)
            .ToString();

        Style = new StyleBuilder()
            .MergeStyle(Modal.Parameters, false)
            .ToString();
    }

    /// <summary>
    /// Called after the component has rendered. Initializes JavaScript interop and opens the dialog on first render.
    /// </summary>
    /// <param name="firstRender"><c>true</c> if this is the first time the component has rendered; otherwise, <c>false</c>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// On first render, this method imports the dialog JavaScript module, creates a dialog manager,
    /// and opens the dialog. Subsequent renders do not reinitialize the dialog.
    /// </remarks>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        // initialize the dialog only on first render
        _module = await JavaScript.InvokeAsync<IJSObjectReference>(
            "import", "./_content/LoreSoft.Blazor.Controls/js/dialog.js");

        _instance = DotNetObjectReference.Create(this);
        _dialog = await _module.InvokeAsync<IJSObjectReference>("createManager", Element, _instance);

        // open the dialog via javascript
        await _dialog.InvokeVoidAsync("open");
    }

    /// <summary>
    /// Handles the <see cref="ModalClose"/> message by programmatically closing the dialog via JavaScript.
    /// </summary>
    /// <param name="close">The modal close message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Uses a special return value of "--closed--" to indicate programmatic closure,
    /// which prevents the <see cref="OnDialogClosed"/> callback from triggering a duplicate close.
    /// </remarks>
    private async Task HandleModalClose(ModalClose close)
    {
        if (_disposed)
            return;

        if (close is null || close.Modal is null || _dialog is null)
            return;

        // close with special value to indicate programmatic close
        await _dialog.InvokeVoidAsync("close", ProgrammaticCloseValue);
    }

    /// <summary>
    /// Disposes the component and releases all JavaScript interop resources.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    /// <remarks>
    /// This method unsubscribes from messenger notifications, disposes JavaScript object references,
    /// and ensures proper cleanup of unmanaged resources. It is safe to call multiple times.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _messenger.Unsubscribe(this);

        if (_dialog != null)
        {
            await _dialog.DisposeAsync();
            _dialog = null;
        }

        if (_module != null)
        {
            await _module.DisposeAsync();
            _module = null;
        }

        _instance?.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
