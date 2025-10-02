// Ignore Spelling: Keydown

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// An input component for selecting dates and times, supporting <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="DateOnly"/>, <see cref="TimeOnly"/>, and <see cref="TimeSpan"/> types.
/// </summary>
/// <typeparam name="TValue">The value type for the picker. Must be a supported date/time type.</typeparam>
public partial class DateTimePicker<TValue> : InputBase<TValue>
{
    /// <summary>
    /// The format string for HTML 'date' inputs.
    /// </summary>
    public const string DateFormat = "yyyy-MM-dd";
    /// <summary>
    /// The format string for HTML 'datetime-local' inputs.
    /// </summary>
    public const string DateTimeLocalFormat = "yyyy-MM-ddTHH:mm:ss";
    /// <summary>
    /// The format string for HTML 'time' inputs.
    /// </summary>
    public const string TimeFormat = "HH:mm:ss";

    private string _parsingErrorMessage = default!;
    private int _month;
    private int _year;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimePicker{TValue}"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <typeparamref name="TValue"/> is not a supported type.</exception>
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

    /// <summary>
    /// Gets or sets the first day of the week for the date picker grid.
    /// </summary>
    [Parameter]
    public DayOfWeek FirstDayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DateTimeFormatInfo"/> used for day and month names.
    /// </summary>
    [Parameter]
    public DateTimeFormatInfo DateTimeFormatInfo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the picker allows clearing the value.
    /// </summary>
    [Parameter]
    public bool AllowClear { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the picker is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the picker mode (date, time, or datetime).
    /// </summary>
    [Parameter]
    public DateTimePickerMode Mode { get; set; }

    /// <summary>
    /// Gets or sets the time scale (in minutes) for time selection.
    /// </summary>
    [Parameter]
    public int TimeScale { get; set; }

    /// <summary>
    /// Gets or sets the error message to display when parsing fails.
    /// </summary>
    [Parameter]
    public string ParsingErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets the list of day-of-week headers for the date picker grid.
    /// </summary>
    protected List<string> Headers { get; set; }

    /// <summary>
    /// Gets the rows of date cells for the date picker grid.
    /// </summary>
    protected List<DatePickerRow> Rows { get; set; }

    /// <summary>
    /// Gets the segments for time selection.
    /// </summary>
    protected List<TimePickerSegment> Segments { get; set; }

    /// <summary>
    /// Gets the reference to the input element.
    /// </summary>
    protected ElementReference DateTimeInput { get; set; }

    /// <summary>
    /// Gets the CSS class for the picker container.
    /// </summary>
    protected string PickerClass => CssBuilder
        .Default("datetime-picker")
        .AddClass("is-date-picker-open", IsDatePickerOpen)
        .ToString();

    /// <summary>
    /// Gets the CSS class for validation state.
    /// </summary>
    protected string? ValidationClass
        => EditContext?.FieldCssClass(FieldIdentifier);

    /// <summary>
    /// Gets the CSS class for the input element.
    /// </summary>
    protected string InputClass => CssBuilder
        .Default("datetime-picker-input")
        .MergeClass(AdditionalAttributes)
        .AddClass(ValidationClass, v => !string.IsNullOrWhiteSpace(v))
        .ToString();

    /// <summary>
    /// Gets or sets the input type for the picker ("date", "time", or "datetime-local").
    /// </summary>
    protected string InputType { get; set; }

    /// <summary>
    /// Gets or sets the format string for the value.
    /// </summary>
    protected string ValueFormat { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to prevent key events (e.g., form submit on Enter).
    /// </summary>
    protected bool PreventKey { get; set; }

    /// <summary>
    /// Gets or sets the current month displayed in the picker.
    /// </summary>
    public int Month
    {
        get => _month;
        set
        {
            _month = value;
            BuildGrid();
        }
    }

    /// <summary>
    /// Gets or sets the current year displayed in the picker.
    /// </summary>
    public int Year
    {
        get => _year;
        set
        {
            _year = value;
            BuildGrid();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the date picker popup is open.
    /// </summary>
    public bool IsDatePickerOpen { get; set; }

    /// <summary>
    /// Gets the current value or today's date/time if the value is not set.
    /// </summary>
    /// <returns>The current value or a default value for the type.</returns>
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

    /// <summary>
    /// Handles keydown events for the input, preventing form submission on Enter.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    public void HandleKeydown(KeyboardEventArgs args)
    {
        // prevent form submit on enter
        PreventKey = args.Key == "Enter";
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <summary>
    /// Builds the date picker grid, including headers and rows.
    /// </summary>
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

    /// <summary>
    /// Sets the picker to today's date and rebuilds the grid.
    /// </summary>
    protected void ShowToday()
    {
        var workingDate = DateTime.Today;

        _year = workingDate.Year;
        _month = workingDate.Month;

        BuildGrid();
    }

    /// <summary>
    /// Moves the picker to the previous month.
    /// </summary>
    protected void PreviousMonth()
    {
        AdjustMonth(-1);
    }

    /// <summary>
    /// Moves the picker to the next month.
    /// </summary>
    protected void NextMonth()
    {
        AdjustMonth(1);
    }

    /// <summary>
    /// Toggles the date picker popup open or closed.
    /// </summary>
    protected void ToggleDatePicker()
    {
        if (IsDatePickerOpen)
            CloseDatePicker();
        else
            ShowDatePicker();
    }

    /// <summary>
    /// Opens the date picker popup and refreshes its content.
    /// </summary>
    protected void ShowDatePicker()
    {
        RefreshDatePicker();
        IsDatePickerOpen = true;
    }

    /// <summary>
    /// Closes the date picker popup.
    /// </summary>
    protected void CloseDatePicker()
    {
        IsDatePickerOpen = false;
    }

    /// <summary>
    /// Refreshes the date picker grid and time segments.
    /// </summary>
    protected void RefreshDatePicker()
    {
        SyncDate(Value);
        BuildGrid();
        BuildTimeSegments();
    }

    /// <summary>
    /// Selects the specified date cell and updates the picker value.
    /// </summary>
    /// <param name="cell">The date cell to select.</param>
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

    /// <summary>
    /// Handles keydown events for a date cell, selecting the date on Enter.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <param name="cell">The date cell.</param>
    protected void DateCellKeyDown(KeyboardEventArgs args, DatePickerCell cell)
    {
        if (args.Key == "Enter")
            SelectDate(cell);
    }

    /// <summary>
    /// Builds the time segments for time selection.
    /// </summary>
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

    /// <summary>
    /// Selects the specified time segment and updates the picker value.
    /// </summary>
    /// <param name="segment">The time segment to select.</param>
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

    /// <summary>
    /// Handles keydown events for a time segment, selecting the time on Enter.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <param name="segment">The time segment.</param>
    protected void TimeCellKeyDown(KeyboardEventArgs args, TimePickerSegment segment)
    {
        if (args.Key == "Enter")
            SelectTime(segment);
    }

    /// <summary>
    /// Handles focus events for the input, opening the date picker.
    /// </summary>
    protected void DateTimeFocus()
    {
        ShowDatePicker();
    }

    /// <summary>
    /// Clears the current value.
    /// </summary>
    protected void ClearValue()
    {
        CurrentValue = default;
    }

    /// <summary>
    /// Determines whether the value can be cleared.
    /// </summary>
    /// <returns><c>true</c> if clearing is allowed and the value is not default; otherwise, <c>false</c>.</returns>
    protected bool CanClear()
    {
        return AllowClear && !EqualityComparer<TValue>.Default.Equals(default, Value);
    }

    /// <summary>
    /// Synchronizes the picker state with the current value.
    /// </summary>
    /// <param name="value">The value to sync.</param>
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

    /// <summary>
    /// Adjusts the displayed month by the specified number of months.
    /// </summary>
    /// <param name="months">The number of months to adjust by.</param>
    private void AdjustMonth(int months)
    {
        var workingDate = new DateTime(Year, Month, 1);
        workingDate = workingDate.AddMonths(months);

        _year = workingDate.Year;
        _month = workingDate.Month;

        BuildGrid();
    }

    /// <summary>
    /// Determines whether the specified date is selected.
    /// </summary>
    /// <param name="workingDate">The date to check.</param>
    /// <returns><c>true</c> if the date is selected; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Determines whether the specified time is selected.
    /// </summary>
    /// <param name="workingDate">The time to check.</param>
    /// <returns><c>true</c> if the time is selected; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Gets the CSS class for a date cell based on its state.
    /// </summary>
    /// <param name="datePickerCell">The date cell.</param>
    /// <returns>The CSS class string.</returns>
    private string DateCellClass(DatePickerCell datePickerCell)
    {
        return CssBuilder
            .Default("date-picker-cell")
            .AddClass("is-primary-month", datePickerCell.IsPrimaryMonth)
            .AddClass("is-secondary-month", datePickerCell.IsSecondaryMonth)
            .AddClass("is-today", datePickerCell.IsToday)
            .AddClass("is-selected", IsDateSelected(datePickerCell.Date))
            .AddClass("is-disabled", datePickerCell.IsDisabled)
            .ToString();
    }

    /// <summary>
    /// Gets the CSS class for a time segment based on its state.
    /// </summary>
    /// <param name="timePickerSegment">The time segment.</param>
    /// <returns>The CSS class string.</returns>
    private string TimeSegmentClass(TimePickerSegment timePickerSegment)
    {
        return CssBuilder
            .Default("time-picker-cell")
            .AddClass("is-selected", IsTimeSelected(timePickerSegment.Date))
            .AddClass("is-disabled", timePickerSegment.IsDisabled)
            .ToString();
    }

    /// <summary>
    /// Gets the shortest day name for the specified <see cref="DayOfWeek"/>.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week.</param>
    /// <returns>The shortest day name string.</returns>
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
