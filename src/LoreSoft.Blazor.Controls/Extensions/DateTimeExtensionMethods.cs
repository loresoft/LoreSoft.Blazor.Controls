namespace LoreSoft.Blazor.Controls.Extensions;

public static class DateTimeExtensionMethods
{
    /// <summary>
    /// Converts a DateTime to a specific timezone.
    /// If the DateTime.Kind is Unspecified, it assumes UTC.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <param name="timeZoneInfo">Target timezone</param>
    /// <returns>DateTime in the specified timezone</returns>
    public static DateTime ToTimeZone(this DateTime dateTime, TimeZoneInfo timeZoneInfo)
    {
        ArgumentNullException.ThrowIfNull(timeZoneInfo);

        return dateTime.Kind switch
        {
            DateTimeKind.Local => TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo),
            DateTimeKind.Utc => TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo),
            DateTimeKind.Unspecified => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), timeZoneInfo),
            _ => TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo)
        };
    }

    /// <summary>
    /// Converts a DateTimeOffset to a specific timezone.
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to convert</param>
    /// <param name="timeZoneInfo">Target timezone</param>
    /// <returns>DateTimeOffset in the specified timezone</returns>
    public static DateTimeOffset ToTimeZone(this DateTimeOffset dateTimeOffset, TimeZoneInfo timeZoneInfo)
    {
        ArgumentNullException.ThrowIfNull(timeZoneInfo);

        return TimeZoneInfo.ConvertTime(dateTimeOffset, timeZoneInfo);
    }
}
