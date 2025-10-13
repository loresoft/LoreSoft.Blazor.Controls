namespace LoreSoft.Blazor.Controls;

public class ModalReference : IModalReference
{
    private readonly Messenger _messenger;
    private readonly TaskCompletionSource<ModalResult> _resultCompletion;

    public ModalReference(
        Messenger messenger,
        Type componentType,
        Dictionary<string, object>? parameters = null,
        string? id = null)
    {
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
        Parameters = parameters ?? [];
        ModalId = id ?? Identifier.Random();

        _resultCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        // Ensure the Modal parameter is set for the component
        Parameters[nameof(ModalComponentBase.Modal)] = this;
    }


    public string ModalId { get; }

    public Type ComponentType { get; }

    public Dictionary<string, object> Parameters { get; }

    public Task<ModalResult> Result => _resultCompletion.Task;


    public async Task CancelAsync()
    {
        await CloseAsync(ModalResult.Cancel());
    }

    public async Task CloseAsync(ModalResult modalResult)
    {
        var message = new ModalClose(this);
        await _messenger.PublishAsync(message);

        Complete(modalResult);
    }

    internal void Complete(ModalResult result)
    {
        _resultCompletion.TrySetResult(result);
    }
}

