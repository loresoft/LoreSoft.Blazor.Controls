using System.Globalization;
using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls;

public partial class DateTimePicker<TValue> : ComponentBase, IDisposable
{
    private readonly EventHandler<ValidationStateChangedEventArgs> _validationStateChangedHandler;
    private bool _previousParsingAttemptFailed;
    private ValidationMessageStore _parsingValidationMessages;
    private Type _nullableUnderlyingType;
    private int _month;
    private int _year;

    public DateTimePicker()
    {
        DateTimeFormatInfo = DateTimeFormatInfo.CurrentInfo;
        DateFormat = "M/d/yyyy";
        TimeFormat = "h:mm tt";
        Headers = new List<string>();
        Rows = new List<DatePickerRow>();
        Segments = new List<TimePickerSegment>();
        AllowClear = true;
        Mode = DateTimePickerMode.Date;
        TimeScale = 30;

        _validationStateChangedHandler = (sender, eventArgs) => StateHasChanged();
    }


    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }


    [Parameter]
    public TValue Value { get; set; }

    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<TValue>> ValueExpression { get; set; }


    [Parameter]
    public DayOfWeek FirstDayOfWeek { get; set; }

    [Parameter]
    public DateTimeFormatInfo DateTimeFormatInfo { get; set; }

    [Parameter]
    public string DateFormat { get; set; }

    [Parameter]
    public string TimeFormat { get; set; }

    [Parameter]
    public bool AllowClear { get; set; }

    [Parameter]
    public bool Disabled { get; set; } = false;

    [Parameter]
    public DateTimePickerMode Mode { get; set; }

    [Parameter]
    public int TimeScale { get; set; }

    [Parameter]
    public FieldIdentifier FieldIdentifier { get; set; }

    [CascadingParameter]
    public EditContext EditContext { get; set; }


    protected List<string> Headers { get; set; }

    protected List<DatePickerRow> Rows { get; set; }

    protected List<TimePickerSegment> Segments { get; set; }


    protected string CurrentValueString
    {
        get => FormatValueAsString(Value);
        set
        {
            _parsingValidationMessages?.Clear();

            bool parsingFailed;

            if (_nullableUnderlyingType != null && string.IsNullOrEmpty(value))
            {
                parsingFailed = false;
                SetValue(default);
            }
            else if (TryParseValueFromString(value, out var parsedValue, out var validationErrorMessage))
            {
                parsingFailed = false;
                SetValue(parsedValue);
            }
            else if (EditContext != null)
            {
                parsingFailed = true;

                if (_parsingValidationMessages == null)
                    _parsingValidationMessages = new ValidationMessageStore(EditContext);

                _parsingValidationMessages.Add(FieldIdentifier, validationErrorMessage);

                EditContext?.NotifyFieldChanged(FieldIdentifier);
            }
            else
            {
                parsingFailed = true;
            }

            SyncDate(Value);
            BuildGrid();

            if (!parsingFailed && !_previousParsingAttemptFailed)
                return;

            EditContext?.NotifyValidationStateChanged();
            _previousParsingAttemptFailed = parsingFailed;
        }
    }

    protected ElementReference DateTimeInput { get; set; }

    protected string ValidationClass
        => EditContext?.FieldCssClass(FieldIdentifier) ?? string.Empty;

    protected string CssClass
    {
        get
        {
            if (AdditionalAttributes != null &&
                AdditionalAttributes.TryGetValue("class", out var cssClss) &&
                !string.IsNullOrEmpty(Convert.ToString(cssClss)))
            {
                return $"{cssClss} {ValidationClass}";
            }

            return ValidationClass; // Never null or empty
        }
    }

    protected bool PreventKey { get; set; }


    public int Month
    {
        get => _month;
        set
        {
            _month = value;
            BuildGrid();
        }
    }

    public int Year
    {
        get => _year;
        set
        {
            _year = value;
            BuildGrid();
        }
    }

    public bool IsDatePickerOpen { get; set; }

    public bool IsTimePickerOpen { get; set; }


    protected override void OnInitialized()
    {
        if (FieldIdentifier.Equals(default))
            FieldIdentifier = FieldIdentifier.Create(ValueExpression);

        _nullableUnderlyingType = Nullable.GetUnderlyingType(typeof(TValue));
    }

    protected void SetValue(TValue value)
    {
        var isEqual = EqualityComparer<TValue>.Default.Equals(value, Value);
        if (isEqual)
            return;

        Value = value;
        ValueChanged.InvokeAsync(value);

        _parsingValidationMessages?.Clear();

        EditContext?.NotifyFieldChanged(FieldIdentifier);
        EditContext?.NotifyValidationStateChanged();
    }

    protected TValue GetValueOrToday()
    {
        if (!EqualityComparer<TValue>.Default.Equals(Value, default))
            return Value;

        var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

        if (targetType == typeof(DateTime))
            return (TValue)(object)DateTime.Now.Date;

        if (targetType == typeof(DateTimeOffset))
            return (TValue)(object)DateTimeOffset.Now.Date;

        if (targetType == typeof(TimeSpan))
            return (TValue)(object)TimeSpan.Zero;

        throw new InvalidOperationException($"The type '{targetType}' is not a supported date type.");
    }

    public void HandleKeydown(KeyboardEventArgs args)
    {
        // prevent form submit on enter
        PreventKey = (args.Key == "Enter");
    }

    protected void BuildGrid()
    {
        Headers.Clear();
        Rows.Clear();

        // start at first of current month
        var workingDate = new DateTime(Year, Month, 1);

        // don't see default date
        if (workingDate == DateTime.MinValue)
        {
            workingDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            _year = workingDate.Year;
            _month = workingDate.Month;
        }

        // walk backward till date is first day of week
        while (workingDate.DayOfWeek != FirstDayOfWeek)
            workingDate = workingDate.AddDays(-1);

        //always build 6 rows
        for (int r = 0; r < 6; r++)
        {
            var row = new DatePickerRow
            {
                Cells = new List<DatePickerCell>()
            };

            // for all days of week
            for (int c = 0; c < DateTimeFormatInfo.DayNames.Length; c++)
            {
                var cell = CreateCell(workingDate);
                row.Cells.Add(cell);

                // create headers if first row
                if (Rows.Count == 0)
                {
                    var dayOfWeek = workingDate.DayOfWeek;
                    var header = GetShortestDayName(dayOfWeek);

                    Headers.Add(header);
                }

                workingDate = workingDate.AddDays(1);
            }

            Rows.Add(row);
        }
    }

    protected void ShowToday()
    {
        var workingDate = DateTime.Today;

        _year = workingDate.Year;
        _month = workingDate.Month;

        BuildGrid();
    }

    protected void PreviousMonth()
    {
        AdjustMonth(-1);
    }

    protected void NextMonth()
    {
        AdjustMonth(1);
    }

    protected void ToggleDatePicker()
    {
        if (IsDatePickerOpen)
            CloseDatePicker();
        else
            ShowDatePicker();
    }

    protected void ShowDatePicker()
    {
        SyncDate(Value);
        BuildGrid();

        IsDatePickerOpen = true;
        IsTimePickerOpen = false;
    }

    protected void CloseDatePicker()
    {
        IsDatePickerOpen = false;
    }

    protected void SelectDate(DatePickerCell cell)
    {
        TValue value = GetValueOrToday();

        switch (value)
        {
            case DateTime dateTimeValue:
            {
                var dateTime = new DateTime(
                    cell.Year,
                    cell.Month,
                    cell.Day,
                    dateTimeValue.Hour,
                    dateTimeValue.Minute,
                    0);

                SetValue((TValue)(object)dateTime);
                break;
            }
            case DateTimeOffset dateTimeOffsetValue:
            {
                var dateTimeOffset = new DateTimeOffset(
                    cell.Year,
                    cell.Month,
                    cell.Day,
                    dateTimeOffsetValue.Hour,
                    dateTimeOffsetValue.Minute,
                    dateTimeOffsetValue.Second,
                    dateTimeOffsetValue.Offset);

                SetValue((TValue)(object)dateTimeOffset);
                break;
            }
            case TimeSpan timeSpanValue:
                break;
        }

        CloseDatePicker();
    }

    protected void DateCellKeyDown(KeyboardEventArgs args, DatePickerCell cell)
    {
        if ((args.Key == "Enter"))
            SelectDate(cell);
    }


    protected void BuildTimeSegments()
    {
        Segments.Clear();
        var workingDate = new DateTime(2000, 1, 1);
        var nextDate = new DateTime(2000, 1, 2);

        while (workingDate < nextDate)
        {
            var segment = new TimePickerSegment
            {
                Hour = workingDate.Hour,
                Minute = workingDate.Minute,
                Text = workingDate.ToString(TimeFormat),
                IsSelected = IsTimeSelected(workingDate),
                CssClass = "timepicker-cell"
            };

            if (segment.IsSelected)
                segment.CssClass += " is-selected";

            if (segment.IsDisabled)
                segment.CssClass += " is-disabled";


            Segments.Add(segment);

            workingDate = workingDate.AddMinutes(TimeScale);
        }
    }

    protected void ToggleTimePicker()
    {
        if (IsTimePickerOpen)
            CloseTimePicker();
        else
            ShowTimePicker();
    }

    protected void ShowTimePicker()
    {
        BuildTimeSegments();

        IsDatePickerOpen = false;
        IsTimePickerOpen = true;
    }

    protected void CloseTimePicker()
    {
        IsTimePickerOpen = false;
    }

    protected void SelectTime(TimePickerSegment segment)
    {
        TValue value = GetValueOrToday();

        switch (value)
        {
            case DateTime dateTimeValue:
                var dateTime = new DateTime(
                    dateTimeValue.Year,
                    dateTimeValue.Month,
                    dateTimeValue.Day,
                    segment.Hour,
                    segment.Minute,
                    0);

                SetValue((TValue)(object)dateTime);
                break;
            case DateTimeOffset dateTimeOffsetValue:
                var dateTimeOffset = new DateTimeOffset(
                    dateTimeOffsetValue.Year,
                    dateTimeOffsetValue.Month,
                    dateTimeOffsetValue.Day,
                    segment.Hour,
                    segment.Minute,
                    0,
                    dateTimeOffsetValue.Offset);

                SetValue((TValue)(object)dateTimeOffset);
                break;
            case TimeSpan timeSpanValue:
                var timeSpan = new TimeSpan(segment.Hour, segment.Minute, 0);
                SetValue((TValue)(object)timeSpan);

                break;
        }

        CloseTimePicker();
    }

    protected void TimeCellKeyDown(KeyboardEventArgs args, TimePickerSegment segment)
    {
        if ((args.Key == "Enter"))
            SelectTime(segment);

    }


    protected void DateTimeFocus()
    {
        if (Mode == DateTimePickerMode.Time)
            ShowTimePicker();
        else
            ShowDatePicker();
    }

    protected void ClearValue()
    {
        SetValue(default);
    }

    protected bool CanClear()
    {
        return AllowClear && !EqualityComparer<TValue>.Default.Equals(default, Value);
    }


    private void SyncDate(TValue value)
    {
        bool isDefault;

        switch (value)
        {
            case DateTime dateTimeValue:
                isDefault = dateTimeValue == DateTime.MinValue;
                _year = isDefault ? DateTime.Now.Year : dateTimeValue.Year;
                _month = isDefault ? DateTime.Now.Month : dateTimeValue.Month;

                break;
            case DateTimeOffset dateTimeOffsetValue:
                isDefault = dateTimeOffsetValue == DateTimeOffset.MinValue;

                _year = isDefault ? DateTime.Now.Year : dateTimeOffsetValue.Year;
                _month = isDefault ? DateTime.Now.Month : dateTimeOffsetValue.Month;

                break;
            default:
                _year = DateTime.Now.Year;
                _month = DateTime.Now.Month;

                break;
        }
    }

    private void AdjustMonth(int months)
    {
        var workingDate = new DateTime(Year, Month, 1);
        workingDate = workingDate.AddMonths(months);

        _year = workingDate.Year;
        _month = workingDate.Month;

        BuildGrid();
    }

    private DatePickerCell CreateCell(DateTime workingDate)
    {
        var cell = new DatePickerCell
        {
            Month = workingDate.Month,
            Year = workingDate.Year,
            Day = workingDate.Day,
            IsToday = workingDate == DateTime.Today,
            IsPrimaryMonth = workingDate.Month == Month,
            IsSecondaryMonth = workingDate.Month != Month,
            IsSelected = IsDateSelected(workingDate),
            CssClass = "datepicker-cell"
        };



        if (cell.IsPrimaryMonth)
            cell.CssClass += " is-primary-month";

        if (cell.IsSecondaryMonth)
            cell.CssClass += " is-secondary-month";

        if (cell.IsToday)
            cell.CssClass += " is-today";

        if (cell.IsSelected)
            cell.CssClass += " is-selected";

        if (cell.IsDisabled)
            cell.CssClass += " is-disabled";

        return cell;
    }

    private bool IsDateSelected(DateTime workingDate)
    {
        TValue value = Value;

        switch (value)
        {
            case DateTime dateTimeValue:
                return workingDate.Date == dateTimeValue.Date;
            case DateTimeOffset dateTimeOffsetValue:
                return workingDate.Date == dateTimeOffsetValue.Date;
            default:
                return false;
        }
    }

    private bool IsTimeSelected(DateTime workingDate)
    {
        TValue value = Value;

        switch (value)
        {
            case DateTime dateTimeValue:
                return workingDate.Hour == dateTimeValue.Hour
                    && workingDate.Minute == dateTimeValue.Minute;
            case DateTimeOffset dateTimeOffsetValue:
                return workingDate.Hour == dateTimeOffsetValue.Hour
                       && workingDate.Minute == dateTimeOffsetValue.Minute;
            case TimeSpan timeSpanValue:
                return workingDate.Hour == timeSpanValue.Hours
                       && workingDate.Minute == timeSpanValue.Minutes;
            default:
                return false;
        }

    }

    private string FormatValueAsString(TValue value)
    {
        // show empty with default value
        if (EqualityComparer<TValue>.Default.Equals(default, Value))
            return string.Empty;

        string format;
        if (Mode == DateTimePickerMode.DateTime)
            format = $"{DateFormat} {TimeFormat}";
        else if (Mode == DateTimePickerMode.Time)
            format = TimeFormat;
        else
            format = DateFormat;

        switch (value)
        {
            case DateTime dateTime:
                return BindConverter.FormatValue(dateTime, format, CultureInfo.InvariantCulture);
            case DateTimeOffset dateTimeOffset:
                return BindConverter.FormatValue(dateTimeOffset, format, CultureInfo.InvariantCulture);
            case TimeSpan timeSpan:
                var date = new DateTime(2000, 1, 1, timeSpan.Hours, timeSpan.Minutes, 0);
                return BindConverter.FormatValue(date, format, CultureInfo.InvariantCulture);
            default:
                return string.Empty; // Handles null for Nullable<DateTime>, etc.
        }
    }

    private bool TryParseValueFromString(string value, out TValue result, out string validationErrorMessage)
    {

        var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

        validationErrorMessage = null;

        bool success;
        if (targetType == typeof(DateTime))
            success = TryParseDateTime(value, out result);
        else if (targetType == typeof(DateTimeOffset))
            success = TryParseDateTimeOffset(value, out result);
        else if (targetType == typeof(TimeSpan))
            success = TryParseTimeSpan(value, out result);
        else
            throw new InvalidOperationException($"The type '{targetType}' is not a supported date type.");

        if (success)
            return true;

        validationErrorMessage = $"The {FieldIdentifier.FieldName} field must be a date.";
        return false;
    }

    private bool TryParseDateTime(string value, out TValue result)
    {
        var success = DateTime.TryParse(value, out var parsedValue);

        result = success ? (TValue)(object)parsedValue : default;

        return success;
    }

    private bool TryParseDateTimeOffset(string value, out TValue result)
    {
        var success = DateTimeOffset.TryParse(value, out var parsedValue);

        result = success ? (TValue)(object)parsedValue : default;

        return success;
    }

    private bool TryParseTimeSpan(string value, out TValue result)
    {
        var success = TimeSpan.TryParse(value, out var parsedValue);
        if (success)
        {
            result = (TValue)(object)parsedValue;
            return true;
        }

        // try parsing as datetime
        success = DateTime.TryParse(value, out var dateTime);
        if (success)
            parsedValue = new TimeSpan(dateTime.Hour, dateTime.Minute, 0);

        result = success ? (TValue)(object)parsedValue : default;

        return success;
    }

    private string GetShortestDayName(DayOfWeek dayOfWeek)
    {
        switch (dayOfWeek)
        {
            case DayOfWeek.Sunday:
                return "Su";
            case DayOfWeek.Monday:
                return "Mo";
            case DayOfWeek.Tuesday:
                return "Tu";
            case DayOfWeek.Wednesday:
                return "We";
            case DayOfWeek.Thursday:
                return "Th";
            case DayOfWeek.Friday:
                return "Fr";
            case DayOfWeek.Saturday:
                return "Sa";
        }

        return string.Empty;
    }

    void IDisposable.Dispose()
    {
        if (EditContext != null)
            EditContext.OnValidationStateChanged -= _validationStateChangedHandler;
    }
}

public enum DateTimePickerMode
{
    Date,
    DateTime,
    Time
}

public class DatePickerRow
{
    public List<DatePickerCell> Cells { get; set; }
}

public class DatePickerCell
{
    public string CssClass { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }

    public bool IsDisabled { get; set; }
    public bool IsSelected { get; set; }
    public bool IsToday { get; set; }
    public bool IsPrimaryMonth { get; set; }
    public bool IsSecondaryMonth { get; set; }
}

public class TimePickerSegment
{
    public string CssClass { get; set; }

    public int Hour { get; set; }
    public int Minute { get; set; }

    public string Text { get; set; }

    public bool IsDisabled { get; set; }
    public bool IsSelected { get; set; }
}
