namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a segment of time for selection in a time picker, including its display text and state.
/// </summary>
public class TimePickerSegment(DateTime date, string text)
{
    /// <summary>
    /// Gets the date and time represented by this segment.
    /// </summary>
    public DateTime Date { get; } = date;

    /// <summary>
    /// Gets the display text for this time segment.
    /// </summary>
    public string Text { get; } = text;

    /// <summary>
    /// Gets the hour component of the time.
    /// </summary>
    public int Hour => Date.Hour;

    /// <summary>
    /// Gets the minute component of the time.
    /// </summary>
    public int Minute => Date.Minute;

    /// <summary>
    /// Gets or sets a value indicating whether this segment is disabled.
    /// </summary>
    public bool IsDisabled { get; set; }
}
