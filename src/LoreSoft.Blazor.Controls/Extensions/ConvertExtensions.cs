#nullable enable

using System.Globalization;

namespace LoreSoft.Blazor.Controls.Extensions;

/// <summary>
/// Converts a string data type to another base data type using a safe conversion method.
/// </summary>
public static class ConvertExtensions
{
    /// <summary>
    /// Converts the specified string representation of a logical value to its Boolean equivalent.
    /// </summary>
    /// <param name="value">A string that contains the value of either <see cref="F:System.Boolean.TrueString"/> or <see cref="F:System.Boolean.FalseString"/>.</param>
    /// <returns>
    /// true if <paramref name="value"/> equals <see cref="F:System.Boolean.TrueString"/>, or false if <paramref name="value"/> equals <see cref="F:System.Boolean.FalseString"/> or null.
    /// </returns>
    public static bool ToBoolean(this string? value)
    {
        if (value == null)
            return false;

        if (bool.TryParse(value, out var result))
            return result;

        string v = value.Trim();

        if (string.Equals(v, "t", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "yes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "x", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "on", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 8-bit unsigned integer, using specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains the number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// An 8-bit unsigned integer that is equivalent to <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static byte? ToByte(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (byte.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of an equivalent date and time, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains a date and time to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// The date and time equivalent of the value of <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static DateTime? ToDateTime(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (DateTime.TryParse(value, out var result))
            return result;

        if (DateTime.TryParseExact(value, "M/d/yyyy hh:mm:ss tt", provider, DateTimeStyles.None, out result))
            return result;

        if (DateTime.TryParseExact(value, "M/d/yyyy", provider, DateTimeStyles.None, out result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of an equivalent <see cref="DateTimeOffset"/>, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains a date and time to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// The date and time equivalent of the value of <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static DateTimeOffset? ToDateTimeOffset(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (DateTimeOffset.TryParse(value, out var result))
            return result;

        if (DateTimeOffset.TryParseExact(value, "M/d/yyyy hh:mm:ss tt", provider, DateTimeStyles.None, out result))
            return result;

        if (DateTimeOffset.TryParseExact(value, "M/d/yyyy", provider, DateTimeStyles.None, out result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent decimal number, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains a number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// A decimal number that is equivalent to the number in <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static decimal? ToDecimal(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (decimal.TryParse(value, NumberStyles.Currency, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent double-precision floating-point number, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains the number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// A double-precision floating-point number that is equivalent to the number in <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static double? ToDouble(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 16-bit signed integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains the number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// A 16-bit signed integer that is equivalent to the number in <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static short? ToInt16(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (short.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 32-bit signed integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains the number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// A 32-bit signed integer that is equivalent to the number in <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static int? ToInt32(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (int.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 64-bit signed integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains the number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// A 64-bit signed integer that is equivalent to the number in <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static long? ToInt64(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (long.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent single-precision floating-point number, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains the number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// A single-precision floating-point number that is equivalent to the number in <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static float? ToSingle(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 16-bit unsigned integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains the number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// A 16-bit unsigned integer that is equivalent to the number in <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static ushort? ToUInt16(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (ushort.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 32-bit unsigned integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains the number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// A 32-bit unsigned integer that is equivalent to the number in <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static uint? ToUInt32(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (uint.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string representation of a number to an equivalent 64-bit unsigned integer, using the specified culture-specific formatting information.
    /// </summary>
    /// <param name="value">A string that contains the number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <returns>
    /// A 64-bit unsigned integer that is equivalent to the number in <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static ulong? ToUInt64(this string? value, IFormatProvider? provider = null)
    {
        if (value == null)
            return null;

        if (ulong.TryParse(value, NumberStyles.Integer, provider, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string to an equivalent <see cref="TimeSpan"/> value.
    /// </summary>
    /// <param name="value">The string representation of a <see cref="TimeSpan"/>.</param>
    /// <returns>
    /// The <see cref="TimeSpan"/> equivalent of the <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static TimeSpan? ToTimeSpan(this string? value)
    {
        if (value == null)
            return null;

        if (TimeSpan.TryParse(value, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Converts the specified string to an equivalent <see cref="Guid"/> value.
    /// </summary>
    /// <param name="value">The string representation of a <see cref="Guid"/>.</param>
    /// <returns>
    /// The <see cref="Guid"/> equivalent of the <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static Guid? ToGuid(this string? value)
    {
        if (value == null)
            return null;

        if (Guid.TryParse(value, out var result))
            return result;

        return null;
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Converts the specified string to an equivalent <see cref="DateOnly"/> value.
    /// </summary>
    /// <param name="value">The string representation of a <see cref="DateOnly"/>.</param>
    /// <returns>
    /// The <see cref="DateOnly"/> equivalent of the <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static DateOnly? ToDateOnly(this string? value)
    {
        if (value == null)
            return null;

        if (DateOnly.TryParse(value, out var dateOnly))
            return dateOnly;

        if (DateTime.TryParse(value, out var dateTime))
            return DateOnly.FromDateTime(dateTime);

        if (DateTimeOffset.TryParse(value, out var dateTimeOffset))
            return DateOnly.FromDateTime(dateTimeOffset.DateTime);

        return null;
    }

    /// <summary>
    /// Converts the specified string to an equivalent <see cref="TimeOnly"/> value.
    /// </summary>
    /// <param name="value">The string representation of a <see cref="TimeOnly"/>.</param>
    /// <returns>
    /// The <see cref="TimeOnly"/> equivalent of the <paramref name="value"/>, or null if value can't be converted.
    /// </returns>
    public static TimeOnly? ToTimeOnly(this string? value)
    {
        if (value == null)
            return null;

        if (TimeOnly.TryParse(value, out var timeOnly))
            return timeOnly;

        if (TimeSpan.TryParse(value, out var timeSpan))
            return TimeOnly.FromTimeSpan(timeSpan);

        if (DateTime.TryParse(value, out var dateTime))
            return TimeOnly.FromDateTime(dateTime);

        if (DateTimeOffset.TryParse(value, out var dateTimeOffset))
            return TimeOnly.FromDateTime(dateTimeOffset.DateTime);

        return null;
    }
#endif


    /// <summary>
    /// Tries to convert the <paramref name="input"/> to the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="input">The input to convert.</param>
    /// <param name="type">The type to convert to.</param>
    /// <returns>The converted value.</returns>
    public static object? SafeConvert(Type type, string? input)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        // first try string
        if (type == typeof(string))
        {
            return input;
        }

        var isNullable = type.IsNullable();
        if ((input?.IsNullOrEmpty() != false) && isNullable)
        {
            return null;
        }

        input = input?.Trim();
        var underlyingType = type.GetUnderlyingType();

        // convert by type
        if (underlyingType == typeof(bool))
        {
            return input.ToBoolean();
        }
        if (underlyingType == typeof(byte))
        {
            var value = input.ToByte();
            return value.HasValue ? value.Value : isNullable ? null : 0;
        }
        if (underlyingType == typeof(DateTime))
        {
            var value = input.ToDateTime();
            return value.HasValue ? value.Value : isNullable ? null : DateTime.MinValue;
        }
        if (underlyingType == typeof(DateTimeOffset))
        {
            var value = input.ToDateTimeOffset();
            return value.HasValue ? value.Value : isNullable ? null : DateTimeOffset.MinValue;
        }
        if (underlyingType == typeof(decimal))
        {
            var value = input.ToDecimal();
            return value.HasValue ? value.Value : isNullable ? null : 0;
        }
        if (underlyingType == typeof(double))
        {
            var value = input.ToDouble();
            return value.HasValue ? value.Value : isNullable ? null : 0;
        }
        if (underlyingType == typeof(short))
        {
            var value = input.ToInt16();
            return value.HasValue ? value.Value : isNullable ? null : 0;
        }
        if (underlyingType == typeof(int))
        {
            var value = input.ToInt32();
            return value.HasValue ? value.Value : isNullable ? null : 0;
        }
        if (underlyingType == typeof(long))
        {
            var value = input.ToInt64();
            return value.HasValue ? value.Value : isNullable ? null : 0;
        }
        if (underlyingType == typeof(float))
        {
            var value = input.ToSingle();
            return value.HasValue ? value.Value : isNullable ? null : 0;
        }
        if (underlyingType == typeof(ushort))
        {
            var value = input.ToUInt16();
            return value.HasValue ? value.Value : isNullable ? null : 0;
        }
        if (underlyingType == typeof(uint))
        {
            var value = input.ToUInt32();
            return value.HasValue ? value.Value : isNullable ? null : 0;
        }
        if (underlyingType == typeof(ulong))
        {
            var value = input.ToUInt64();
            return value.HasValue ? value.Value : isNullable ? null : 0;
        }
        if (underlyingType == typeof(TimeSpan))
        {
            var value = input.ToTimeSpan();
            return value.HasValue ? value.Value : isNullable ? null : TimeSpan.Zero;
        }
        if (underlyingType == typeof(Guid))
        {
            var value = input.ToGuid();
            return value.HasValue ? value.Value : isNullable ? null : Guid.Empty;
        }
#if NET6_0_OR_GREATER
        if (underlyingType == typeof(DateOnly))
        {
            var value = input.ToDateOnly();
            return value.HasValue ? value.Value : isNullable ? null : DateOnly.MinValue;
        }
        if (underlyingType == typeof(TimeOnly))
        {
            var value = input.ToTimeOnly();
            return value.HasValue ? value.Value : isNullable ? null : TimeOnly.MinValue;
        }
#endif
        return default;
    }


    /// <summary>
    /// Converts the result to the TValue type.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="convert">The optional convert function.</param>
    /// <returns>The converted value.</returns>
    public static TValue? ConvertValue<TValue>(object? result, Func<object?, TValue>? convert = null)
    {
        if (result is null || result == DBNull.Value)
            return default;

        if (result is TValue valueType)
            return valueType;

        if (convert != null)
            return convert(result);

        if (result is string stringValue)
            return (TValue?)SafeConvert(typeof(TValue), stringValue);

        return (TValue)Convert.ChangeType(result, typeof(TValue));
    }
}
