namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Specifies the mode of the <see cref="DateTimePicker{TValue}"/> component.
/// </summary>
public enum DateTimePickerMode
{
    /// <summary>
    /// The picker allows selection of a date only (year, month, day).
    /// </summary>
    Date,

    /// <summary>
    /// The picker allows selection of both date and time (year, month, day, hour, minute, second).
    /// </summary>
    DateTime,

    /// <summary>
    /// The picker allows selection of time only (hour, minute, second).
    /// </summary>
    Time
}
