namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Service for displaying and managing modal dialogs in Blazor applications.
/// </summary>
/// <remarks>
/// The <see cref="ModalService"/> provides a simple API for showing modal dialogs by publishing
/// <see cref="ModalShow"/> messages through the messenger service. Register this service in your
/// DI container to use it throughout your application.
/// </remarks>
public class ModalService(Messenger messenger)
{
    private readonly Messenger _messenger = messenger
        ?? throw new ArgumentNullException(nameof(messenger));

    /// <summary>
    /// Shows a modal dialog with the specified component and optional parameters.
    /// </summary>
    /// <typeparam name="TComponent">The type of the modal component to display. Must derive from <see cref="ModalComponentBase"/>.</typeparam>
    /// <param name="parameters">Optional dictionary of parameters to pass to the modal component. If <c>null</c>, a default parameter set is created.</param>
    /// <returns>
    /// A task that returns an <see cref="IModalReference"/> which can be used to interact with the modal
    /// and await its result.
    /// </returns>
    /// <remarks>
    /// This method creates a <see cref="ModalReference"/>, publishes a <see cref="ModalShow"/> message to display the modal,
    /// and returns the reference which allows the caller to await the modal's result or programmatically close it.
    /// </remarks>
    /// <example>
    /// <code>
    /// var modal = await modalService.Show&lt;MyModalComponent&gt;();
    /// var result = await modal.Result;
    /// </code>
    /// </example>
    public async Task<IModalReference> Show<TComponent>(IDictionary<string, object>? parameters = null)
        where TComponent : ModalComponentBase
    {
        // ensure parameters is not null
        parameters ??= CreateParameters(string.Empty);

        // modal reference controls the lifetime of the modal
        var modalReference = new ModalReference(_messenger, typeof(TComponent), parameters);

        // publish a message to show the modal
        var message = new ModalShow(modalReference);
        await _messenger.PublishAsync(message);

        // allow caller to await the result
        return modalReference;
    }

    /// <summary>
    /// Creates a parameter dictionary with common modal component properties.
    /// </summary>
    /// <param name="message">The message text to display in the modal.</param>
    /// <param name="title">The title of the modal. Defaults to "Alert".</param>
    /// <param name="type">The visual variant of the modal. Defaults to <see cref="ModalVariant.Primary"/>.</param>
    /// <param name="primaryAction">The text for the primary action button. Defaults to "OK".</param>
    /// <param name="secondaryAction">The text for the secondary action button. Defaults to "Cancel".</param>
    /// <returns>A dictionary containing the parameter names and values for a modal component.</returns>
    /// <remarks>
    /// This helper method creates a standardized parameter dictionary that can be passed to the <see cref="Show{TComponent}"/> method.
    /// The parameter names match the properties defined in <see cref="ModalComponentBase"/>.
    /// </remarks>
    public static Dictionary<string, object> CreateParameters(
        string message,
        string title = "Alert",
        ModalVariant type = ModalVariant.Primary,
        string primaryAction = "OK",
        string secondaryAction = "Cancel")
    {
        return new Dictionary<string, object>
        {
            { nameof(ModalComponentBase.Title), title },
            { nameof(ModalComponentBase.Message), message },
            { nameof(ModalComponentBase.Variant), type },
            { nameof(ModalComponentBase.PrimaryAction), primaryAction },
            { nameof(ModalComponentBase.SecondaryAction), secondaryAction }
        };
    }
}
