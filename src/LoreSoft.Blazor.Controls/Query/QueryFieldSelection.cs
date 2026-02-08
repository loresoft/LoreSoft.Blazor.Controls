namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Specifies the mode used for which field name is used, column name or property name for filtering or sorting
/// </summary>
public enum QueryFieldSelection
{
    /// <summary>
    /// Use the column name in a data source, such as a database or a data table.
    /// </summary>
    Column,
    /// <summary>
    /// Use the property name as defined in code, such as a class property.
    /// </summary>
    Property
}
