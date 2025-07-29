namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a cell in a date picker, encapsulating a specific date and its display state.
/// </summary>
public class DatePickerCell(DateTime date)
{
    /// <summary>
    /// Gets the date represented by this cell.
    /// </summary>
    public DateTime Date { get; } = date;

    /// <summary>
    /// Gets the year component of the date.
    /// </summary>
    public int Year => Date.Year;

    /// <summary>
    /// Gets the month component of the date.
    /// </summary>
    public int Month => Date.Month;

    /// <summary>
    /// Gets the day component of the date.
    /// </summary>
    public int Day => Date.Day;

    /// <summary>
    /// Gets a value indicating whether this cell represents today's date.
    /// </summary>
    public bool IsToday => Date == DateTime.Today;

    /// <summary>
    /// Gets a value indicating whether this cell is in the primary month being displayed.
    /// </summary>
    public bool IsPrimaryMonth => Date.Month == Month;

    /// <summary>
    /// Gets a value indicating whether this cell is in a secondary month (not the primary month).
    /// </summary>
    public bool IsSecondaryMonth => Date.Month != Month;

    /// <summary>
    /// Gets or sets a value indicating whether this cell is disabled.
    /// </summary>
    public bool IsDisabled { get; set; }
}
