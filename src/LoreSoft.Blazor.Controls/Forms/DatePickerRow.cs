namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a row in a date picker, containing a collection of <see cref="DatePickerCell"/> instances.
/// </summary>
public class DatePickerRow
{
    /// <summary>
    /// Gets a unique key for the row, useful for rendering and identification.
    /// </summary>
    public string Key { get; } = Identifier.Random();

    /// <summary>
    /// Gets the collection of cells in this row.
    /// </summary>
    public List<DatePickerCell> Cells { get; } = [];
}
