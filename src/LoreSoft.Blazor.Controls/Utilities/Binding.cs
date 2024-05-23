using System.Globalization;
using System.Reflection;

using LoreSoft.Blazor.Controls.Extensions;

using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls.Utilities;

public static class Binding
{
    public static string Format(object value)
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

    public static object Convert(object value, Type type)
    {
        var stringValue = value as string;

        return ConvertExtensions.SafeConvert(type, stringValue);
    }

    public static object Convert<T>(object value)
    {
        return Convert(value, typeof(T));
    }
}
