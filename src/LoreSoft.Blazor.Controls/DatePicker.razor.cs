using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace LoreSoft.Blazor.Controls
{
    public class DatePickerBase<TValue> : ComponentBase, IDisposable
    {
        private readonly EventHandler<ValidationStateChangedEventArgs> _validationStateChangedHandler;
        private bool _previousParsingAttemptFailed;
        private ValidationMessageStore _parsingValidationMessages;
        private Type _nullableUnderlyingType;
        private int _month;
        private int _year;

        public DatePickerBase()
        {
            DateTimeFormatInfo = DateTimeFormatInfo.CurrentInfo;
            DateFormat = "MM/dd/yyyy";
            Headers = new List<string>();
            Rows = new List<DatePickerRow>();
            AllowClear = true;

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
        public bool AllowClear { get; set; }

        [Parameter]
        public bool Disabled { get; set; } = false;

        [Parameter]
        public FieldIdentifier FieldIdentifier { get; set; }

        [CascadingParameter]
        public EditContext EditContext { get; set; }

        public List<string> Headers { get; set; }

        public List<DatePickerRow> Rows { get; set; }


        protected TValue CurrentValue
        {
            get => Value;
            set
            {
                var isEqual = EqualityComparer<TValue>.Default.Equals(value, Value);
                if (isEqual)
                    return;

                Value = value;
                ValueChanged.InvokeAsync(value);
                EditContext?.NotifyFieldChanged(FieldIdentifier);
            }
        }

        protected string CurrentValueString
        {
            get => FormatValueAsString(CurrentValue);
            set
            {
                _parsingValidationMessages?.Clear();

                bool parsingFailed;

                if (_nullableUnderlyingType != null && string.IsNullOrEmpty(value))
                {
                    parsingFailed = false;
                    CurrentValue = default;
                }
                else if (TryParseValueFromString(value, out var parsedValue, out var validationErrorMessage))
                {
                    parsingFailed = false;
                    CurrentValue = parsedValue;
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

                SyncDate(CurrentValue);
                BuildGrid();

                if (!parsingFailed && !_previousParsingAttemptFailed)
                    return;

                EditContext?.NotifyValidationStateChanged();
                _previousParsingAttemptFailed = parsingFailed;
            }
        }


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

        public bool IsModalOpen { get; set; }


        protected override void OnInitialized()
        {
            if (FieldIdentifier.Equals(default))
                FieldIdentifier = FieldIdentifier.Create(ValueExpression);
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
                        var header = DateTimeFormatInfo.GetShortestDayName(dayOfWeek);

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

        protected void ShowPicker()
        {
            SyncDate(CurrentValue);
            BuildGrid();

            IsModalOpen = true;
        }

        protected void ClosePicker()
        {
            IsModalOpen = false;
        }

        protected void SelectDate(DatePickerCell cell)
        {
            // set Year, Month and 
            var date = new DateTime(cell.Year, cell.Month, cell.Day);
            CurrentValue = (TValue)(object)date;

            IsModalOpen = false;
        }

        protected void HandleKeyDown(KeyboardEventArgs args, DatePickerCell cell)
        {
            if ((args.Key == "Enter"))
                SelectDate(cell);
        }

        protected void ClearValue()
        {
            CurrentValue = default;
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
                IsSelected = IsSelected(workingDate),
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

        private bool IsSelected(DateTime workingDate)
        {
            TValue value = CurrentValue;

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


        private string FormatValueAsString(TValue value)
        {
            // show empty with default datetime
            if (EqualityComparer<TValue>.Default.Equals(default, Value))
                return string.Empty;

            switch (value)
            {
                case DateTime dateTimeValue:
                    return BindConverter.FormatValue(dateTimeValue, DateFormat, CultureInfo.InvariantCulture);
                case DateTimeOffset dateTimeOffsetValue:
                    return BindConverter.FormatValue(dateTimeOffsetValue, DateFormat, CultureInfo.InvariantCulture);
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


        void IDisposable.Dispose()
        {
            if (EditContext != null)
                EditContext.OnValidationStateChanged -= _validationStateChangedHandler;
        }
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
}
