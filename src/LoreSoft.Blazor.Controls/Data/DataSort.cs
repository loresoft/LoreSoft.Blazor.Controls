namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Represents a sorting instruction for a data query in a Blazor data-bound component.
/// </summary>
/// <param name="Property">The name of the property to sort by.</param>
/// <param name="Descending">True to sort in descending order; false for ascending order.</param>
public record DataSort(string Property, bool Descending);
