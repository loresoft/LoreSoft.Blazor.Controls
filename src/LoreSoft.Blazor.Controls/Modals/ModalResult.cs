#nullable enable

namespace LoreSoft.Blazor.Controls;

public readonly record struct ModalResult
{
    public ModalResult(object? data, bool cancelled)
    {
        Data = data;
        Cancelled = cancelled;
    }

    public object? Data { get; }

    public bool Cancelled { get; }

    public readonly bool Confirmed => !Cancelled;


    public static ModalResult Ok()
        => new(default, false);

    public static ModalResult Ok(object? result)
        => new(result, false);

    public static ModalResult Cancel()
        => new(default, true);

    public static ModalResult Cancel(object? result)
        => new(result, true);
}

