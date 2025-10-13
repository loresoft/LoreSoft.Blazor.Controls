using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

public partial class ModalDialog : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private IJSObjectReference? _dialog;
    private DotNetObjectReference<ModalDialog>? _instance;

    [Inject]
    public IJSRuntime JavaScript { get; set; } = null!;

    [Parameter]
    public ModalReference Modal { get; set; } = null!;

    public ElementReference Element { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _module = await JavaScript.InvokeAsync<IJSObjectReference>(
            "import", "./_content/LoreSoft.Blazor.Controls/js/dialog.js");

        _instance = DotNetObjectReference.Create(this);
        _dialog = await _module.InvokeAsync<IJSObjectReference>("createManager", Element, _instance);

        await _dialog.InvokeVoidAsync("open");
    }

    [JSInvokable]
    public async Task OnDialogClosed(string? returnValue)
    {
        await Modal.CloseAsync(ModalResult.Cancel());
    }

    public async ValueTask DisposeAsync()
    {
        if (_dialog != null)
        {
            await _dialog.InvokeVoidAsync("dispose");
            await _dialog.DisposeAsync();
        }

        if (_module != null)
            await _module.DisposeAsync();

        _instance?.Dispose();

        GC.SuppressFinalize(this);
    }
}
