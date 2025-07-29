namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides constants for supported query operators used in filtering and searching within data-bound components.
/// </summary>
public static class QueryOperators
{
    /// <summary>
    /// Represents the "equal" operator for comparing values.
    /// </summary>
    public const string Equal = "equal";

    /// <summary>
    /// Represents the "not equal" operator for comparing values.
    /// </summary>
    public const string NotEqual = "not equal";

    /// <summary>
    /// Represents the "contains" operator for substring or collection membership checks.
    /// </summary>
    public const string Contains = "contains";

    /// <summary>
    /// Represents the "not contains" operator for substring or collection membership checks.
    /// </summary>
    public const string NotContains = "not contains";

    /// <summary>
    /// Represents the "starts with" operator for prefix matching.
    /// </summary>
    public const string StartsWith = "starts with";

    /// <summary>
    /// Represents the "not starts with" operator for prefix matching.
    /// </summary>
    public const string NotStartsWith = "not starts with";

    /// <summary>
    /// Represents the "ends with" operator for suffix matching.
    /// </summary>
    public const string EndsWith = "ends with";

    /// <summary>
    /// Represents the "not ends with" operator for suffix matching.
    /// </summary>
    public const string NotEndsWith = "not ends with";

    /// <summary>
    /// Represents the "greater than" operator for numeric or date comparisons.
    /// </summary>
    public const string GreaterThan = "greater than";

    /// <summary>
    /// Represents the "greater than or equal" operator for numeric or date comparisons.
    /// </summary>
    public const string GreaterThanOrEqual = "greater than or equal";

    /// <summary>
    /// Represents the "less than" operator for numeric or date comparisons.
    /// </summary>
    public const string LessThan = "less than";

    /// <summary>
    /// Represents the "less than or equal" operator for numeric or date comparisons.
    /// </summary>
    public const string LessThanOrEqual = "less than or equal";

    /// <summary>
    /// Represents the "is null" operator for checking if a value is null.
    /// </summary>
    public const string IsNull = "is null";

    /// <summary>
    /// Represents the "is not null" operator for checking if a value is not null.
    /// </summary>
    public const string IsNotNull = "is not null";
}
