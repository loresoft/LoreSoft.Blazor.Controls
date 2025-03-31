using LoreSoft.Blazor.Controls.Utilities;

namespace LoreSoft.Blazor.Controls;

public class DatePickerCell(DateTime date)
{
    public DateTime Date { get; } = date;

    public int Year => Date.Year;
    public int Month => Date.Month;
    public int Day => Date.Day;

    public bool IsToday => Date == DateTime.Today;
    public bool IsPrimaryMonth => Date.Month == Month;
    public bool IsSecondaryMonth => Date.Month != Month;

    public bool IsDisabled { get; set; }
}
