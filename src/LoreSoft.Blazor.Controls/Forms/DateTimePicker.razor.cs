// Ignore Spelling: Keydown

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls;

public partial class DateTimePicker<TValue> : InputBase<TValue>
{
    private const string DateFormat = "yyyy-MM-dd";                     // Compatible with HTML 'date' inputs
    private const string DateTimeLocalFormat = "yyyy-MM-ddTHH:mm:ss";   // Compatible with HTML 'datetime-local' inputs
    private const string TimeFormat = "HH:mm:ss";                       // Compatible with HTML 'time' inputs

    private string _parsingErrorMessage = default!;

    private int _month;
    private int _year;

    public DateTimePicker()
    {
        DateTimeFormatInfo = DateTimeFormatInfo.CurrentInfo;
        Headers = [];
        Rows = [];
        Segments = [];
        AllowClear = true;
        TimeScale = 30;

        var type = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly))
        {
            Mode = DateTimePickerMode.Date;
            InputType = "date";
            ValueFormat = DateFormat;
        }
        else if (type == typeof(TimeOnly) || type == typeof(TimeSpan))
        {
            Mode = DateTimePickerMode.Time;
            InputType = "time";
            ValueFormat = TimeFormat;
        }
        else
        {
            throw new InvalidOperationException($"Unsupported {GetType()} type param '{type}'.");
        }
    }


    [Parameter]
    public DayOfWeek FirstDayOfWeek { get; set; }

    [Parameter]
    public DateTimeFormatInfo DateTimeFormatInfo { get; set; }

    [Parameter]
    public bool AllowClear { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public DateTimePickerMode Mode { get; set; }

    [Parameter]
    public int TimeScale { get; set; }

    [Parameter]
    public string ParsingErrorMessage { get; set; } = string.Empty;


    protected List<string> Headers { get; set; }

    protected List<DatePickerRow> Rows { get; set; }

    protected List<TimePickerSegment> Segments { get; set; }


    protected ElementReference DateTimeInput { get; set; }

    protected string PickerClass => CssBuilder
        .Default("datetimepicker")
        .AddClass("is-datepicker-open", IsDatePickerOpen)
        .ToString();

    protected string? ValidationClass
        => EditContext?.FieldCssClass(FieldIdentifier);

    protected string InputClass => CssBuilder
        .Default("datetimepicker-input")
        .MergeClass(AdditionalAttributes)
        .AddClass(ValidationClass, v => !string.IsNullOrWhiteSpace(v))
        .ToString();

    protected string InputType { get; set; }

    protected string ValueFormat { get; set; }

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


    protected TValue? GetValueOrToday()
    {
        if (!EqualityComparer<TValue>.Default.Equals(Value, default))
            return Value;

        var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

        if (targetType == typeof(DateOnly))
            return (TValue)(object)DateOnly.FromDateTime(DateTime.Now);

        if (targetType == typeof(DateTime))
            return (TValue)(object)DateTime.Now.Date;

        if (targetType == typeof(DateTimeOffset))
            return (TValue)(object)DateTimeOffset.Now.Date;

        if (targetType == typeof(TimeOnly))
            return (TValue)(object)TimeOnly.MinValue;

        if (targetType == typeof(TimeSpan))
            return (TValue)(object)TimeSpan.Zero;

        throw new InvalidOperationException($"The type '{targetType}' is not a supported date type.");
    }

    public void HandleKeydown(KeyboardEventArgs args)
    {
        // prevent form submit on enter
        PreventKey = args.Key == "Enter";
    }


    protected override void OnParametersSet()
    {
        (InputType, ValueFormat, var formatDescription) = Mode switch
        {
            DateTimePickerMode.Date => ("date", DateFormat, "date"),
            DateTimePickerMode.DateTime => ("datetime-local", DateTimeLocalFormat, "date and time"),
            DateTimePickerMode.Time => ("time", TimeFormat, "time"),
            _ => throw new InvalidOperationException($"Unsupported {nameof(DateTimePickerMode)} '{Mode}'.")
        };

        _parsingErrorMessage = string.IsNullOrEmpty(ParsingErrorMessage)
            ? $"The {{0}} field must be a {formatDescription}."
            : ParsingErrorMessage;
    }

    protected override string FormatValueAsString(TValue? value)
    {
        return value switch
        {
            DateTime dateTimeValue => BindConverter.FormatValue(dateTimeValue, ValueFormat, CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffsetValue => BindConverter.FormatValue(dateTimeOffsetValue, ValueFormat, CultureInfo.InvariantCulture),
            DateOnly dateOnlyValue => BindConverter.FormatValue(dateOnlyValue, ValueFormat, CultureInfo.InvariantCulture),
            TimeOnly timeOnlyValue => BindConverter.FormatValue(timeOnlyValue, ValueFormat, CultureInfo.InvariantCulture),
            _ => string.Empty, // Handles null for Nullable<DateTime>, etc.
        };
    }

    protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out TValue result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (BindConverter.TryConvertTo(value, CultureInfo.InvariantCulture, out result))
        {
            Debug.Assert(result != null);
            validationErrorMessage = null;
            return true;
        }
        else
        {
            validationErrorMessage = string.Format(CultureInfo.InvariantCulture, _parsingErrorMessage, DisplayName ?? FieldIdentifier.FieldName);
            return false;
        }
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
            var row = new DatePickerRow();

            // for all days of week
            for (int c = 0; c < DateTimeFormatInfo.DayNames.Length; c++)
            {
                var cell = new DatePickerCell(workingDate);
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
        RefreshDatePicker();
        IsDatePickerOpen = true;
    }

    protected void CloseDatePicker()
    {
        IsDatePickerOpen = false;
    }

    protected void RefreshDatePicker()
    {
        SyncDate(Value);
        BuildGrid();
        BuildTimeSegments();
    }

    protected void SelectDate(DatePickerCell cell)
    {
        var value = GetValueOrToday();

        switch (value)
        {
            case DateOnly:
            {
                var date = new DateOnly(
                    cell.Year,
                    cell.Month,
                    cell.Day);

                CurrentValue = (TValue)(object)date;
                break;
            }
            case DateTime dateTimeValue:
            {
                var dateTime = new DateTime(
                    cell.Year,
                    cell.Month,
                    cell.Day,
                    dateTimeValue.Hour,
                    dateTimeValue.Minute,
                    0);

                CurrentValue = (TValue)(object)dateTime;
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

                CurrentValue = (TValue)(object)dateTimeOffset;
                break;
            }
            case TimeSpan:
                break;
        }
    }

    protected void DateCellKeyDown(KeyboardEventArgs args, DatePickerCell cell)
    {
        if (args.Key == "Enter")
            SelectDate(cell);
    }


    protected void BuildTimeSegments()
    {
        Segments.Clear();
        var workingDate = new DateTime(2000, 1, 1);
        var nextDate = new DateTime(2000, 1, 2);

        while (workingDate < nextDate)
        {
            var segment = new TimePickerSegment(workingDate, workingDate.ToString("h:mm tt"));

            Segments.Add(segment);

            workingDate = workingDate.AddMinutes(TimeScale);
        }
    }

    protected void SelectTime(TimePickerSegment segment)
    {
        var value = GetValueOrToday();

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

                CurrentValue = (TValue)(object)dateTime;
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

                CurrentValue = (TValue)(object)dateTimeOffset;
                break;
            case TimeSpan:
                var timeSpan = new TimeSpan(segment.Hour, segment.Minute, 0);
                CurrentValue = (TValue)(object)timeSpan;

                break;
            case TimeOnly:
                var timeOnly = new TimeOnly(segment.Hour, segment.Minute, 0);
                CurrentValue = (TValue)(object)timeOnly;

                break;
        }
    }

    protected void TimeCellKeyDown(KeyboardEventArgs args, TimePickerSegment segment)
    {
        if (args.Key == "Enter")
            SelectTime(segment);

    }


    protected void DateTimeFocus()
    {
        ShowDatePicker();
    }

    protected void ClearValue()
    {
        CurrentValue = default;
    }

    protected bool CanClear()
    {
        return AllowClear && !EqualityComparer<TValue>.Default.Equals(default, Value);
    }


    private void SyncDate(TValue? value)
    {
        bool isDefault;

        switch (value)
        {
            case DateOnly dateOnly:
                isDefault = dateOnly == DateOnly.MinValue;
                _year = isDefault ? DateTime.Now.Year : dateOnly.Year;
                _month = isDefault ? DateTime.Now.Month : dateOnly.Month;

                break;
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


    private bool IsDateSelected(DateTime workingDate)
    {
        var value = Value;

        return value switch
        {
            DateOnly dateOnly => DateOnly.FromDateTime(workingDate.Date) == dateOnly,
            DateTime dateTimeValue => workingDate.Date == dateTimeValue.Date,
            DateTimeOffset dateTimeOffsetValue => workingDate.Date == dateTimeOffsetValue.Date,
            _ => false,
        };
    }

    private bool IsTimeSelected(DateTime workingDate)
    {
        var value = Value;

        return value switch
        {
            DateTime dateTimeValue => workingDate.Hour == dateTimeValue.Hour && workingDate.Minute == dateTimeValue.Minute,
            DateTimeOffset dateTimeOffsetValue => workingDate.Hour == dateTimeOffsetValue.Hour && workingDate.Minute == dateTimeOffsetValue.Minute,
            TimeOnly timeOnly => workingDate.Hour == timeOnly.Hour && workingDate.Minute == timeOnly.Minute,
            TimeSpan timeSpanValue => workingDate.Hour == timeSpanValue.Hours && workingDate.Minute == timeSpanValue.Minutes,
            _ => false,
        };
    }


    private string DateCellClass(DatePickerCell datePickerCell)
    {
        return CssBuilder
            .Default("datepicker-cell")
            .AddClass("is-primary-month", datePickerCell.IsPrimaryMonth)
            .AddClass("is-secondary-month", datePickerCell.IsSecondaryMonth)
            .AddClass("is-today", datePickerCell.IsToday)
            .AddClass("is-selected", IsDateSelected(datePickerCell.Date))
            .AddClass("is-disabled", datePickerCell.IsDisabled)
            .ToString();
    }

    private string TimeSegmentClass(TimePickerSegment timePickerSegment)
    {
        return CssBuilder
            .Default("timepicker-cell")
            .AddClass("is-selected", IsTimeSelected(timePickerSegment.Date))
            .AddClass("is-disabled", timePickerSegment.IsDisabled)
            .ToString();
    }


    private static string GetShortestDayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Sunday => "Su",
            DayOfWeek.Monday => "Mo",
            DayOfWeek.Tuesday => "Tu",
            DayOfWeek.Wednesday => "We",
            DayOfWeek.Thursday => "Th",
            DayOfWeek.Friday => "Fr",
            DayOfWeek.Saturday => "Sa",
            _ => string.Empty,
        };
    }
}
