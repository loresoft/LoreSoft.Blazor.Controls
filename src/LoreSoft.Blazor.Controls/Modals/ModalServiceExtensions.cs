namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides extension methods for <see cref="ModalService"/> to display common modal dialog types.
/// </summary>
/// <remarks>
/// These extension methods simplify showing common modal dialogs such as confirm, alert, and prompt dialogs
/// with convenient parameter overloads and result handling.
/// </remarks>
public static class ModalServiceExtensions
{
    /// <summary>
    /// Shows a confirmation modal dialog with two action buttons.
    /// </summary>
    /// <param name="modalService">The modal service instance.</param>
    /// <param name="message">The message text to display in the confirmation dialog.</param>
    /// <param name="title">The title of the dialog. Defaults to "Confirm".</param>
    /// <param name="type">The visual variant of the dialog. Defaults to <see cref="ModalVariant.Primary"/>.</param>
    /// <param name="primaryAction">The text for the primary action button. Defaults to "OK".</param>
    /// <param name="secondaryAction">The text for the secondary action button. Defaults to "Cancel".</param>
    /// <returns>
    /// A task that returns an <see cref="IModalReference"/> which can be used to interact with the modal and await its result.
    /// </returns>
    /// <remarks>
    /// This method displays a <see cref="ConfirmModal"/> component with the specified parameters.
    /// </remarks>
    public static async Task<IModalReference> ConfirmModal(
        this ModalService modalService,
        string message,
        string title = "Confirm",
        ModalVariant type = ModalVariant.Primary,
        string primaryAction = "OK",
        string secondaryAction = "Cancel")
    {
        var parameters = ModalService.CreateParameters(
            message: message,
            title: title,
            type: type,
            primaryAction: primaryAction,
            secondaryAction: secondaryAction);

        return await modalService.Show<ConfirmModal>(parameters);
    }

    /// <summary>
    /// Shows a confirmation modal dialog and returns whether the user confirmed the action.
    /// </summary>
    /// <param name="modalService">The modal service instance.</param>
    /// <param name="message">The message text to display in the confirmation dialog.</param>
    /// <param name="title">The title of the dialog. Defaults to "Confirm".</param>
    /// <param name="type">The visual variant of the dialog. Defaults to <see cref="ModalVariant.Primary"/>.</param>
    /// <param name="primaryAction">The text for the primary action button. Defaults to "OK".</param>
    /// <param name="secondaryAction">The text for the secondary action button. Defaults to "Cancel".</param>
    /// <returns>
    /// A task that returns <c>true</c> if the user confirmed the action; otherwise, <c>false</c> if cancelled.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that shows a confirmation dialog and automatically awaits the result,
    /// returning a boolean indicating whether the dialog was confirmed or cancelled.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (await modalService.Confirm("Are you sure you want to delete this item?"))
    /// {
    ///     // User confirmed, proceed with deletion
    /// }
    /// </code>
    /// </example>
    public static async Task<bool> Confirm(
        this ModalService modalService,
        string message,
        string title = "Confirm",
        ModalVariant type = ModalVariant.Primary,
        string primaryAction = "OK",
        string secondaryAction = "Cancel")
    {
        var modal = await modalService.ConfirmModal(
            message: message,
            title: title,
            type: type,
            primaryAction: primaryAction,
            secondaryAction: secondaryAction
        );

        var result = await modal.Result;
        return !result.Cancelled;
    }

    /// <summary>
    /// Shows a confirmation modal dialog for deleting an item.
    /// </summary>
    /// <param name="modalService">The modal service instance.</param>
    /// <param name="itemName">The name of the item to delete. Defaults to "this item".</param>
    /// <returns>
    /// A task that returns <c>true</c> if the user confirmed the deletion; otherwise, <c>false</c> if cancelled.
    /// </returns>
    public static Task<bool> ConfirmDelete(
        this ModalService modalService,
        string itemName = "this item")
    {
        return modalService.Confirm(
            message: $"Are you sure you want to delete {itemName}?",
            title: "Confirm Deletion",
            type: ModalVariant.Danger,
            primaryAction: "Delete",
            secondaryAction: "Cancel"
        );
    }

    /// <summary>
    /// Shows an alert modal dialog with a single action button.
    /// </summary>
    /// <param name="modalService">The modal service instance.</param>
    /// <param name="message">The message text to display in the alert dialog.</param>
    /// <param name="title">The title of the dialog. Defaults to "Alert".</param>
    /// <param name="type">The visual variant of the dialog. Defaults to <see cref="ModalVariant.Primary"/>.</param>
    /// <param name="primaryAction">The text for the primary action button. Defaults to "OK".</param>
    /// <returns>
    /// A task that returns an <see cref="IModalReference"/> which can be used to interact with the modal and await its result.
    /// </returns>
    /// <remarks>
    /// This method displays an <see cref="AlertModal"/> component with the specified parameters.
    /// </remarks>
    public static async Task<IModalReference> AlertModal(
        this ModalService modalService,
        string message,
        string title = "Alert",
        ModalVariant type = ModalVariant.Primary,
        string primaryAction = "OK")
    {
        var parameters = ModalService.CreateParameters(
            message: message,
            title: title,
            type: type,
            primaryAction: primaryAction);

        return await modalService.Show<AlertModal>(parameters);
    }

    /// <summary>
    /// Shows an alert modal dialog and returns whether the user acknowledged the alert.
    /// </summary>
    /// <param name="modalService">The modal service instance.</param>
    /// <param name="message">The message text to display in the alert dialog.</param>
    /// <param name="title">The title of the dialog. Defaults to "Alert".</param>
    /// <param name="type">The visual variant of the dialog. Defaults to <see cref="ModalVariant.Primary"/>.</param>
    /// <param name="primaryAction">The text for the primary action button. Defaults to "OK".</param>
    /// <returns>
    /// A task that returns <c>true</c> if the user acknowledged the alert; otherwise, <c>false</c> if cancelled.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that shows an alert dialog and automatically awaits the result,
    /// returning a boolean indicating whether the dialog was acknowledged or dismissed.
    /// </remarks>
    /// <example>
    /// <code>
    /// await modalService.Alert("Operation completed successfully!", type: ModalVariant.Success);
    /// </code>
    /// </example>
    public static async Task<bool> Alert(
        this ModalService modalService,
        string message,
        string title = "Alert",
        ModalVariant type = ModalVariant.Primary,
        string primaryAction = "OK")
    {
        var modal = await modalService.AlertModal(
            message: message,
            title: title,
            type: type,
            primaryAction: primaryAction
        );

        var result = await modal.Result;
        return !result.Cancelled;
    }

    /// <summary>
    /// Shows a prompt modal dialog that requests text input from the user.
    /// </summary>
    /// <param name="modalService">The modal service instance.</param>
    /// <param name="message">The message text to display in the prompt dialog.</param>
    /// <param name="title">The title of the dialog. Defaults to "Prompt".</param>
    /// <param name="defaultValue">The default value to pre-populate in the input field. Defaults to an empty string.</param>
    /// <param name="type">The visual variant of the dialog. Defaults to <see cref="ModalVariant.Primary"/>.</param>
    /// <param name="primaryAction">The text for the primary action button. Defaults to "OK".</param>
    /// <param name="secondaryAction">The text for the secondary action button. Defaults to "Cancel".</param>
    /// <returns>
    /// A task that returns an <see cref="IModalReference"/> which can be used to interact with the modal and await its result.
    /// </returns>
    /// <remarks>
    /// This method displays a <see cref="PromptModal"/> component with the specified parameters,
    /// including a default value for the input field.
    /// </remarks>
    public static async Task<IModalReference> PromptModal(
        this ModalService modalService,
        string message,
        string title = "Prompt",
        string defaultValue = "",
        ModalVariant type = ModalVariant.Primary,
        string primaryAction = "OK",
        string secondaryAction = "Cancel")
    {
        var parameters = ModalService.CreateParameters(
            message: message,
            title: title,
            type: type,
            primaryAction: primaryAction,
            secondaryAction: secondaryAction);

        parameters[nameof(Controls.PromptModal.DefaultValue)] = defaultValue;

        return await modalService.Show<PromptModal>(parameters);
    }

    /// <summary>
    /// Shows a prompt modal dialog and returns the text input provided by the user.
    /// </summary>
    /// <param name="modalService">The modal service instance.</param>
    /// <param name="message">The message text to display in the prompt dialog.</param>
    /// <param name="title">The title of the dialog. Defaults to "Prompt".</param>
    /// <param name="defaultValue">The default value to pre-populate in the input field. Defaults to an empty string.</param>
    /// <param name="type">The visual variant of the dialog. Defaults to <see cref="ModalVariant.Primary"/>.</param>
    /// <param name="primaryAction">The text for the primary action button. Defaults to "OK".</param>
    /// <param name="secondaryAction">The text for the secondary action button. Defaults to "Cancel".</param>
    /// <returns>
    /// A task that returns the user's input text if confirmed; otherwise, <c>null</c> if cancelled.
    /// </returns>
    /// <remarks>
    /// This is a convenience method that shows a prompt dialog and automatically awaits the result,
    /// returning the user's input string if confirmed, or <c>null</c> if the dialog was cancelled.
    /// </remarks>
    /// <example>
    /// <code>
    /// var name = await modalService.Prompt("Please enter your name:", defaultValue: "John Doe");
    /// if (name != null)
    /// {
    ///     // User provided input
    ///     Console.WriteLine($"Hello, {name}!");
    /// }
    /// </code>
    /// </example>
    public static async Task<string?> Prompt(
        this ModalService modalService,
        string message,
        string title = "Prompt",
        string defaultValue = "",
        ModalVariant type = ModalVariant.Primary,
        string primaryAction = "OK",
        string secondaryAction = "Cancel")
    {
        var modal = await modalService.PromptModal(
            message: message,
            title: title,
            defaultValue: defaultValue,
            type: type,
            primaryAction: primaryAction,
            secondaryAction: secondaryAction
        );

        var result = await modal.Result;
        if (result.Cancelled)
            return default;

        return result.Data as string;
    }
}
