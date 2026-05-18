namespace LoreSoft.Blazor.Controls.Events;

/// <summary>
/// Describes the parameter signature of a weak event handler method, used to
/// select the correct invocation path when publishing an event.
/// </summary>
/// <remarks>
/// The numeric values are intentionally chosen so the bit-encoding
/// <c>(supportParameter ? 1 : 0) | (supportCancellation ? 2 : 0)</c> maps directly
/// onto the corresponding enum member.
/// </remarks>
internal enum InvocationMode : byte
{
    /// <summary>
    /// No parameters: <c>void M()</c>, <c>ValueTask M()</c>, or <c>Task M()</c>.
    /// </summary>
    None = 0,

    /// <summary>
    /// A single event-data parameter: <c>M(TEvent)</c>.
    /// </summary>
    Parameter = 1,

    /// <summary>
    /// A single <see cref="System.Threading.CancellationToken"/> parameter:
    /// <c>M(CancellationToken)</c>.
    /// </summary>
    Cancellation = 2,

    /// <summary>
    /// Event-data plus <see cref="System.Threading.CancellationToken"/>:
    /// <c>M(TEvent, CancellationToken)</c>.
    /// </summary>
    ParameterAndCancellation = 3,
}
