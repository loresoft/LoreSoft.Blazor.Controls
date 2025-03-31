using LoreSoft.Blazor.Controls.Utilities;

namespace LoreSoft.Blazor.Controls;

public class TimePickerSegment(DateTime date, string text)
{
    public DateTime Date { get; } = date;
    public string Text { get; } = text;

    public int Hour => Date.Hour;
    public int Minute => Date.Minute;

    public bool IsDisabled { get; set; }
}
