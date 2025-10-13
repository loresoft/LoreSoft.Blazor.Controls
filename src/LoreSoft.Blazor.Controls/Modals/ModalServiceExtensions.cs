namespace LoreSoft.Blazor.Controls;

public static class ModalServiceExtensions
{
    public static async Task<IModalReference> ConfirmModal(
        this ModalService modalService,
        string message,
        string title = "Confirm",
        ModalVariant type = ModalVariant.Primary,
        string primaryAction = "OK",
        string secondaryAction = "Cancel")
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(ModalComponentBase.Message), message },
            { nameof(ModalComponentBase.Title), title },
            { nameof(ModalComponentBase.Variant), type },
            { nameof(ModalComponentBase.PrimaryAction), primaryAction },
            { nameof(ModalComponentBase.SecondaryAction), secondaryAction }
        };

        return await modalService.Show<ConfirmModal>(parameters);
    }

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

    public static async Task<IModalReference> AlertModal(
        this ModalService modalService,
        string message,
        string title = "Alert",
        ModalVariant type = ModalVariant.Primary,
        string primaryAction = "OK")
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(ModalComponentBase.Message), message },
            { nameof(ModalComponentBase.Title), title },
            { nameof(ModalComponentBase.Variant), type },
            { nameof(ModalComponentBase.PrimaryAction), primaryAction }
        };

        return await modalService.Show<AlertModal>(parameters);
    }

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

    public static async Task<IModalReference> PromptModal(
        this ModalService modalService,
        string message,
        string title = "Prompt",
        string defaultValue = "",
        ModalVariant type = ModalVariant.Primary,
        string primaryAction = "OK",
        string secondaryAction = "Cancel")
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(ModalComponentBase.Message), message },
            { nameof(ModalComponentBase.Title), title },
            { nameof(ModalComponentBase.Variant), type },
            { nameof(ModalComponentBase.PrimaryAction), primaryAction },
            { nameof(ModalComponentBase.SecondaryAction), secondaryAction },
            { nameof(Controls.PromptModal.DefaultValue), defaultValue },
        };

        return await modalService.Show<PromptModal>(parameters);
    }

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
