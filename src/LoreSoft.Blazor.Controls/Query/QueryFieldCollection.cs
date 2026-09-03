namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Holds the field definitions registered by <see cref="QueryBuilderTemplate"/> child components.
/// </summary>
/// <remarks>
/// The collection is cascaded to the field components so registration is independent of the
/// <see cref="QueryBuilder"/> lifetime. This allows a host component, such as <see cref="DataList{TItem}"/>,
/// to know the available fields even when the query builder user interface has not been rendered yet.
/// </remarks>
public class QueryFieldCollection
{
    /// <summary>
    /// Gets the registered field definitions.
    /// </summary>
    public List<QueryBuilderTemplate> Fields { get; } = [];

    /// <summary>
    /// Adds a field definition to the collection when it is not already registered.
    /// </summary>
    /// <param name="field">The <see cref="QueryBuilderTemplate"/> instance to add.</param>
    public void Add(QueryBuilderTemplate field)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (Fields.Contains(field))
            return;

        Fields.Add(field);
    }

    /// <summary>
    /// Removes a field definition from the collection.
    /// </summary>
    /// <param name="field">The <see cref="QueryBuilderTemplate"/> instance to remove.</param>
    public void Remove(QueryBuilderTemplate field)
    {
        ArgumentNullException.ThrowIfNull(field);

        Fields.Remove(field);
    }
}
