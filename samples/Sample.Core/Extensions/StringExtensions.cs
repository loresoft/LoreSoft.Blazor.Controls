using System;

namespace Sample.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return !string.IsNullOrEmpty(source)
                && !string.IsNullOrEmpty(value)
                && source.IndexOf(value, comparisonType) >= 0;
        }
    }
}
