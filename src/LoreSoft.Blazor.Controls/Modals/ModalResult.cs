namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents the result of a modal dialog operation.
/// </summary>
public readonly record struct ModalResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModalResult"/> struct.
    /// </summary>
    /// <param name="data">The data associated with the modal result.</param>
    /// <param name="cancelled">A value indicating whether the modal was cancelled.</param>
    public ModalResult(object? data, bool cancelled)
    {
        Data = data;
        Cancelled = cancelled;
    }

    /// <summary>
    /// Gets the data associated with the modal result.
    /// </summary>
    /// <value>The data object, or <c>null</c> if no data was provided.</value>
    public object? Data { get; }

    /// <summary>
    /// Gets a value indicating whether the modal dialog was cancelled.
    /// </summary>
    /// <value><c>true</c> if the modal was cancelled; otherwise, <c>false</c>.</value>
    public bool Cancelled { get; }

    /// <summary>
    /// Gets a value indicating whether the modal dialog was confirmed (not cancelled).
    /// </summary>
    /// <value><c>true</c> if the modal was confirmed; otherwise, <c>false</c>.</value>
    public readonly bool Confirmed => !Cancelled;

    /// <summary>
    /// Creates a successful modal result without data.
    /// </summary>
    /// <returns>A <see cref="ModalResult"/> indicating success with no data.</returns>
    public static ModalResult Success()
        => new(default, false);

    /// <summary>
    /// Creates a successful modal result with the specified data.
    /// </summary>
    /// <param name="result">The data to include in the result.</param>
    /// <returns>A <see cref="ModalResult"/> indicating success with the specified data.</returns>
    public static ModalResult Success(object? result)
        => new(result, false);

    /// <summary>
    /// Creates a cancelled modal result without data.
    /// </summary>
    /// <returns>A <see cref="ModalResult"/> indicating cancellation with no data.</returns>
    public static ModalResult Cancel()
        => new(default, true);

    /// <summary>
    /// Creates a cancelled modal result with the specified data.
    /// </summary>
    /// <param name="result">The data to include in the result.</param>
    /// <returns>A <see cref="ModalResult"/> indicating cancellation with the specified data.</returns>
    public static ModalResult Cancel(object? result)
        => new(result, true);
}

