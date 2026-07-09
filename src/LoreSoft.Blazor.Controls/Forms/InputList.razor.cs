using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;

using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// An input component for editing a list of values.
/// </summary>
/// <typeparam name="TValue">The list item value type.</typeparam>
public partial class InputList<TValue> : ComponentBase, IDisposable
{
    private static readonly Type _nullableUnderlyingType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
    private ValidationMessageStore? _parsingValidationMessages;
    private EditContext? _previousEditContext;
    private Expression<Func<IList<TValue>>>? _previousValueExpression;

    /// <summary>
    /// Gets or sets the cascading edit context.
    /// </summary>
    [CascadingParameter]
    public EditContext? EditContext { get; set; }

    /// <summary>
    /// Gets or sets additional attributes applied to the input list container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the display name used in validation messages.
    /// </summary>
    [Parameter]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to each generated input.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to each remove button.
    /// </summary>
    [Parameter]
    public string? RemoveClass { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the add button.
    /// </summary>
    [Parameter]
    public string? AddClass { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the list inputs are disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether item edits update on input instead of change.
    /// </summary>
    [Parameter]
    public bool UpdateOnInput { get; set; }

    /// <summary>
    /// Gets or sets the HTML input type. When not set, the type is inferred from <typeparamref name="TValue" />.
    /// </summary>
    [Parameter]
    public string? InputType { get; set; }

    /// <summary>
    /// Gets or sets the value to use when adding a new item.
    /// </summary>
    [Parameter]
    public TValue? NewItemValue { get; set; }

    /// <summary>
    /// Gets or sets a factory used to create new items.
    /// </summary>
    [Parameter]
    public Func<TValue>? NewItemFactory { get; set; }

    /// <summary>
    /// Gets or sets the add button title.
    /// </summary>
    [Parameter]
    public string AddTitle { get; set; } = "Add item";

    /// <summary>
    /// Gets or sets the add button text.
    /// </summary>
    [Parameter]
    public string AddText { get; set; } = "Add";

    /// <summary>
    /// Gets or sets the add button content.
    /// </summary>
    [Parameter]
    public RenderFragment? AddContent { get; set; }

    /// <summary>
    /// Gets or sets the remove button title.
    /// </summary>
    [Parameter]
    public string RemoveTitle { get; set; } = "Remove item";

    /// <summary>
    /// Gets or sets the remove button text.
    /// </summary>
    [Parameter]
    public string RemoveText { get; set; } = "Remove";

    /// <summary>
    /// Gets or sets the remove button content.
    /// </summary>
    [Parameter]
    public RenderFragment? RemoveContent { get; set; }


    /// <summary>
    /// Gets or sets the current list value.
    /// </summary>
    [Parameter]
    public IList<TValue>? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the list changes.
    /// </summary>
    [Parameter]
    public EventCallback<IList<TValue>> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the value expression used for validation.
    /// </summary>
    [Parameter]
    public Expression<Func<IList<TValue>>>? ValueExpression { get; set; }


    /// <summary>
    /// Gets the field identifier used for validation.
    /// </summary>
    protected FieldIdentifier FieldIdentifier { get; private set; }

    /// <summary>
    /// Gets or sets the current list value.
    /// </summary>
    protected IList<TValue> CurrentValue { get; set; } = [];

    /// <summary>
    /// Gets additional attributes for the container with the class attribute removed.
    /// </summary>
    protected IReadOnlyDictionary<string, object>? ContainerAttributes => RemoveClassAttribute(AdditionalAttributes);

    /// <summary>
    /// Gets the HTML input type used by item inputs.
    /// </summary>
    protected string EffectiveInputType => InputType ?? GetDefaultInputType();


    /// <summary>
    /// Updates the current list when parameters are set.
    /// </summary>
    protected override void OnParametersSet()
    {
        // copy the list reference to CurrentValue to avoid modifying the bound Value directly, which could cause issues with change detection and validation.
        CurrentValue = Value ?? [];

        // If the EditContext or ValueExpression has changed, we need to update the FieldIdentifier and clear any existing parsing errors.
        if (!ReferenceEquals(_previousEditContext, EditContext) || !ReferenceEquals(_previousValueExpression, ValueExpression))
        {
            ClearParsingError(_previousEditContext, FieldIdentifier, notifyValidationStateChanged: true);

            _previousEditContext = EditContext;
            _previousValueExpression = ValueExpression;

            FieldIdentifier = ValueExpression != null
                ? FieldIdentifier.Create(ValueExpression)
                : default;

            _parsingValidationMessages = null;
        }

        // If the FieldIdentifier is not set and we have a ValueExpression, create the FieldIdentifier from the ValueExpression.
        if (FieldIdentifier.Equals(default) && ValueExpression != null)
            FieldIdentifier = FieldIdentifier.Create(ValueExpression);
    }


    /// <summary>
    /// Gets the CSS class for the overall input list container.
    /// </summary>
    /// <returns>The container CSS class.</returns>
    protected string? ContainerClass()
    {
        // If we have an EditContext and a valid FieldIdentifier, get the validation CSS class for the field.
        // This will apply classes like "valid" or "invalid" based on the field's validation state.
        var validationClass = EditContext != null && !FieldIdentifier.Equals(default)
            ? EditContext.FieldCssClass(FieldIdentifier)
            : string.Empty;

        return CssBuilder.Pool.Use(builder => builder
            .AddClass("input-list")
            .AddClass(validationClass, !string.IsNullOrWhiteSpace(validationClass))
            .AddClass(AdditionalAttributes?.TryGetValue("class", out var classValue) == true ? classValue?.ToString() : null)
            .ToString()
        );
    }

    /// <summary>
    /// Gets the CSS class for each generated input.
    /// </summary>
    /// <returns>The input CSS class.</returns>
    protected string? ItemInputClass()
        => CssBuilder.Pool.Use(builder => builder
            .AddClass("input-list-input")
            .AddClass(InputClass)
            .ToString()
        );

    /// <summary>
    /// Gets the CSS class for each remove button.
    /// </summary>
    /// <returns>The remove button CSS class.</returns>
    protected string? RemoveButtonClass()
        => CssBuilder.Pool.Use(builder => builder
            .AddClass("input-list-remove")
            .AddClass(RemoveClass)
            .ToString()
        );

    /// <summary>
    /// Gets the CSS class for the add button.
    /// </summary>
    /// <returns>The add button CSS class.</returns>
    protected string? AddButtonClass()
        => CssBuilder.Pool.Use(builder => builder
            .AddClass("input-list-add")
            .AddClass(AddClass)
            .ToString()
        );


    /// <summary>
    /// Formats an item for rendering in an input value attribute.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted value.</returns>
    protected string? FormatValue(TValue? value)
    {
        if (value is null)
            return null;

        if (_nullableUnderlyingType == typeof(DateTime) && value is DateTime dateTime)
        {
            return EffectiveInputType == "datetime-local"
                ? dateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
                : dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (_nullableUnderlyingType == typeof(DateTimeOffset) && value is DateTimeOffset dateTimeOffset)
        {
            return EffectiveInputType == "datetime-local"
                ? dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
                : dateTimeOffset.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (_nullableUnderlyingType == typeof(DateOnly) && value is DateOnly dateOnly)
            return dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (_nullableUnderlyingType == typeof(TimeOnly) && value is TimeOnly timeOnly)
            return timeOnly.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        if (_nullableUnderlyingType == typeof(TimeSpan) && value is TimeSpan timeSpan)
            return timeSpan.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);

        return BindConverter.FormatValue(value, GetValueCulture())?.ToString();
    }


    /// <summary>
    /// Adds a new item to the list.
    /// </summary>
    protected async Task AddItem()
    {
        var list = CreateMutableValue();

        TValue? item = CreateNewItem();
        list.Add(item);

        ClearParsingError();
        await NotifyValueChanged(list).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes an item from the list.
    /// </summary>
    /// <param name="index">The item index to remove.</param>
    protected async Task RemoveItem(int index)
    {
        if (index < 0 || index >= CurrentValue.Count)
            return;

        var list = CreateMutableValue();

        if (index >= list.Count)
            return;

        list.RemoveAt(index);

        ClearParsingError();
        await NotifyValueChanged(list).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles item input events when <see cref="UpdateOnInput" /> is enabled.
    /// </summary>
    /// <param name="index">The item index.</param>
    /// <param name="args">The change event arguments.</param>
    protected async Task HandleInput(int index, ChangeEventArgs args)
    {
        if (!UpdateOnInput)
            return;

        await UpdateItem(index, args.Value).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles item change events.
    /// </summary>
    /// <param name="index">The item index.</param>
    /// <param name="args">The change event arguments.</param>
    protected async Task HandleChange(int index, ChangeEventArgs args)
    {
        if (UpdateOnInput)
            return;

        await UpdateItem(index, args.Value).ConfigureAwait(false);
    }


    private List<TValue> CreateMutableValue()
        => [.. CurrentValue];

    private TValue CreateNewItem()
    {
        if (NewItemFactory != null)
            return NewItemFactory();

        if (NewItemValue is not null)
            return NewItemValue;

        if (typeof(TValue) == typeof(string))
            return (TValue)(object)string.Empty;

        return default!;
    }

    private async Task UpdateItem(int index, object? value)
    {
        if (index < 0 || index >= CurrentValue.Count)
            return;

        if (!TryParseValue(value, out var parsedValue, out var validationErrorMessage))
        {
            AddParsingError(validationErrorMessage);
            NotifyFieldChanged();
            return;
        }

        var list = CreateMutableValue();

        if (index >= list.Count)
            return;

        list[index] = parsedValue!;

        ClearParsingError();
        await NotifyValueChanged(list).ConfigureAwait(false);
    }

    private async Task NotifyValueChanged(IList<TValue> value)
    {
        Value = value;
        CurrentValue = value;

        await ValueChanged.InvokeAsync(value).ConfigureAwait(false);

        NotifyFieldChanged();
    }

    private void NotifyFieldChanged()
    {
        if (EditContext == null || FieldIdentifier.Equals(default))
            return;

        EditContext.NotifyFieldChanged(FieldIdentifier);
        EditContext.NotifyValidationStateChanged();
    }

    private void AddParsingError(string? validationErrorMessage)
    {
        if (EditContext == null || FieldIdentifier.Equals(default))
            return;

        _parsingValidationMessages ??= new ValidationMessageStore(EditContext);
        _parsingValidationMessages.Clear(FieldIdentifier);
        _parsingValidationMessages.Add(FieldIdentifier, validationErrorMessage ?? GetParsingErrorMessage());
    }

    private void ClearParsingError()
        => ClearParsingError(EditContext, FieldIdentifier, notifyValidationStateChanged: false);

    private void ClearParsingError(EditContext? editContext, FieldIdentifier fieldIdentifier, bool notifyValidationStateChanged)
    {
        if (_parsingValidationMessages == null || editContext == null || fieldIdentifier.Equals(default))
            return;

        _parsingValidationMessages.Clear(fieldIdentifier);

        if (notifyValidationStateChanged)
            editContext.NotifyValidationStateChanged();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ClearParsingError(EditContext, FieldIdentifier, notifyValidationStateChanged: true);
        _parsingValidationMessages = null;

        GC.SuppressFinalize(this);
    }

    private bool TryParseValue(
        object? value,
        [MaybeNull] out TValue result,
        [NotNullWhen(false)] out string? validationErrorMessage)
    {
        var valueString = value?.ToString();

        if (string.IsNullOrEmpty(valueString) && Nullable.GetUnderlyingType(typeof(TValue)) != null)
        {
            result = default;
            validationErrorMessage = null;
            return true;
        }

        if (typeof(TValue) == typeof(string))
        {
            result = (TValue)(object)(valueString ?? string.Empty);
            validationErrorMessage = null;
            return true;
        }

        if (typeof(TValue) == typeof(Uri))
        {
            if (Uri.TryCreate(valueString ?? string.Empty, UriKind.RelativeOrAbsolute, out var uri))
            {
                result = (TValue)(object)uri;
                validationErrorMessage = null;
                return true;
            }

            result = default;
            validationErrorMessage = GetParsingErrorMessage();
            return false;
        }

        if (BindConverter.TryConvertTo<TValue>(valueString, GetValueCulture(), out var convertedValue))
        {
            result = convertedValue;
            validationErrorMessage = null;
            return true;
        }

        result = default;
        validationErrorMessage = GetParsingErrorMessage();
        return false;
    }

    private string GetParsingErrorMessage()
        => $"The {DisplayName ?? FieldIdentifier.FieldName} field is not valid.";

    private CultureInfo GetValueCulture()
    {
        var inputType = EffectiveInputType;

        return inputType is "number" or "date" or "time" or "datetime-local" or "month" or "week"
            ? CultureInfo.InvariantCulture
            : CultureInfo.CurrentCulture;
    }

    private static string GetDefaultInputType()
    {
        if (IsNumericType(_nullableUnderlyingType))
            return "number";

        if (_nullableUnderlyingType == typeof(DateTime) || _nullableUnderlyingType == typeof(DateTimeOffset))
            return "datetime-local";

        if (_nullableUnderlyingType == typeof(DateOnly))
            return "date";

        if (_nullableUnderlyingType == typeof(TimeOnly) || _nullableUnderlyingType == typeof(TimeSpan))
            return "time";

        return "text";
    }

    private static bool IsNumericType(Type type)
        => type == typeof(byte)
            || type == typeof(sbyte)
            || type == typeof(short)
            || type == typeof(ushort)
            || type == typeof(int)
            || type == typeof(uint)
            || type == typeof(long)
            || type == typeof(ulong)
            || type == typeof(float)
            || type == typeof(double)
            || type == typeof(decimal);

    private static Dictionary<string, object>? RemoveClassAttribute(Dictionary<string, object>? attributes)
    {
        if (attributes == null || !attributes.ContainsKey("class"))
            return attributes;

        return attributes
            .Where(static pair => pair.Key != "class")
            .ToDictionary(static pair => pair.Key, static pair => pair.Value);
    }
}
