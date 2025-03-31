namespace LoreSoft.Blazor.Controls;

public class DatePickerRow
{
    public string Key { get; } = Guid.NewGuid().ToString();
    public List<DatePickerCell> Cells { get; } = [];
}
