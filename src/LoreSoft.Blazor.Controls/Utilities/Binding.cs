using System.Globalization;

using LoreSoft.Blazor.Controls.Extensions;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides utility methods for formatting and converting values for data binding in components.
/// </summary>
public static class Binding
{
    /// <summary>
    /// Formats an object value as a string suitable for data binding.
    /// Handles common types such as string, bool, numeric types, and date/time types.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>
    /// The formatted string representation of the value, or <c>null</c> if the value is <c>null</c>.
    /// </returns>
    public static string? Format(object? value)
    {
        return value switch
        {
            null => null,
            string stringValue => stringValue,
            bool boolValue => boolValue ? "true" : "false",
            int intValue => BindConverter.FormatValue(intValue, CultureInfo.InvariantCulture),
            long longValue => BindConverter.FormatValue(longValue, CultureInfo.InvariantCulture),
            short shortValue => BindConverter.FormatValue(shortValue, CultureInfo.InvariantCulture),
            float floatValue => BindConverter.FormatValue(floatValue, CultureInfo.InvariantCulture),
            double doubleValue => BindConverter.FormatValue(doubleValue, CultureInfo.InvariantCulture),
            decimal decimalValue => BindConverter.FormatValue(decimalValue, CultureInfo.InvariantCulture),
            DateTime dateTimeValue => BindConverter.FormatValue(dateTimeValue, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffsetValue => BindConverter.FormatValue(dateTimeOffsetValue, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateOnly dateOnlyValue => BindConverter.FormatValue(dateOnlyValue, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimeOnly timeOnlyValue => BindConverter.FormatValue(timeOnlyValue, "HH:mm:ss", CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }

    /// <summary>
    /// Converts a value to the specified type using safe conversion logic.
    /// </summary>
    /// <param name="value">The value to convert, typically a string.</param>
    /// <param name="type">The target type to convert to.</param>
    /// <returns>
    /// The converted value as the specified type, or <c>null</c> if conversion fails.
    /// </returns>
    public static object? Convert(object? value, Type type)
    {
        var stringValue = value as string;

        return ConvertExtensions.SafeConvert(type, stringValue);
    }

    /// <summary>
    /// Converts a value to the specified generic type using safe conversion logic.
    /// </summary>
    /// <typeparam name="T">The target type to convert to.</typeparam>
    /// <param name="value">The value to convert, typically a string.</param>
    /// <returns>
    /// The converted value as type <typeparamref name="T"/>, or <c>null</c> if conversion fails.
    /// </returns>
    public static object? Convert<T>(object? value)
    {
        return Convert(value, typeof(T));
    }
}
