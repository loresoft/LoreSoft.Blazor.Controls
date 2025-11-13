#nullable enable

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides utility methods for generating unique HTML element identifiers.
/// </summary>
public static class Identifier
{
    private static int _defaultCounter = 0;
    private static readonly ConcurrentDictionary<string, StrongBox<int>> _counters = new();

    /// <summary>
    /// Generates a random HTML element identifier.
    /// </summary>
    /// <param name="prefix">The prefix for the identifier. Default is "id".</param>
    /// <returns>A random identifier in the format "{prefix}-{hex}".</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="prefix"/> is null or whitespace.</exception>
    public static string Random(string prefix = "id")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        return $"{prefix}-{System.Random.Shared.Next():x8}";
    }

    /// <summary>
    /// Generates a sequential, sortable HTML element identifier.
    /// </summary>
    /// <param name="prefix">The prefix for the identifier. Default is "id".</param>
    /// <returns>A sequential identifier in the format "{prefix}-{hex}" that is lexicographically sortable.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="prefix"/> is null or whitespace.</exception>
    /// <remarks>
    /// <para>
    /// This method is thread-safe and uses an atomic counter per prefix to ensure unique sequential identifiers.
    /// Each prefix maintains its own independent counter, so identifiers with different prefixes start from 1.
    /// The identifiers are formatted as hexadecimal values for compactness and sortability.
    /// </para>
    /// <para>
    /// Performance: The default "id" prefix is optimized with a dedicated counter to avoid dictionary lookup overhead,
    /// making it the most performant option for typical use cases.
    /// </para>
    /// <para>
    /// The counter will automatically reset to 1 when approaching the maximum int value to prevent overflow.
    /// </para>
    /// </remarks>
    public static string Sequential(string prefix = "id")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        // Optimize for the default "id" prefix to avoid dictionary lookup
        ref int counter = ref prefix == "id"
            ? ref _defaultCounter
            : ref _counters.GetOrAdd(prefix, static _ => new StrongBox<int>(0)).Value;

        int value = Interlocked.Increment(ref counter);

        // Reset counter if approaching max value (leave some headroom for thread safety)
        if (value >= int.MaxValue - 1000)
            Interlocked.Exchange(ref counter, 0);

        return $"{prefix}-{value:x8}";
    }
}
