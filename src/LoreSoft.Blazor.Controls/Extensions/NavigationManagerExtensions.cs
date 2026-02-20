using Microsoft.AspNetCore.Components;

namespace LoreSoft.Blazor.Controls.Extensions;

/// <summary>
/// Extension methods for <see cref="NavigationManager"/>.
/// </summary>
public static class NavigationManagerExtensions
{
    // FNV-1a 32-bit constants
    private const uint FnvPrime = 16777619;
    private const uint FnvOffsetBasis = 2166136261;

    /// <summary>
    /// Returns an 8-character lowercase hex string that is a stable, case-insensitive FNV-1a hash
    /// of the current relative URL path. Safe to use as a storage key component.
    /// </summary>
    /// <remarks>
    /// The path is normalized before hashing: it is lowercased and trailing slashes are stripped,
    /// so <c>/Orders/</c> and <c>/orders</c> produce the same hash.
    /// </remarks>
    /// <param name="navigationManager">The <see cref="NavigationManager"/> instance.</param>
    /// <param name="includeQuery">
    /// When <see langword="true"/>, the query string (e.g. <c>?page=2</c>) is included in the hash.
    /// Defaults to <see langword="false"/> so the same state key is used regardless of query parameters.
    /// </param>
    /// <param name="includeFragment">
    /// When <see langword="true"/>, the URL fragment (e.g. <c>#section</c>) is included in the hash.
    /// </param>
    /// <returns>An 8-character lowercase hex string uniquely identifying the current URL path.</returns>
    public static string GetPathHash(
        this NavigationManager navigationManager,
        bool includeQuery = false,
        bool includeFragment = false)
    {
        ArgumentNullException.ThrowIfNull(navigationManager);

        var relative = navigationManager.ToBaseRelativePath(navigationManager.Uri);

        var queryIndex = relative.IndexOf('?');
        var fragmentIndex = relative.IndexOf('#');

        // Path ends at ? or #, whichever comes first
        var pathEnd = queryIndex >= 0 ? queryIndex : (fragmentIndex >= 0 ? fragmentIndex : relative.Length);

        // Normalize: lowercase and strip trailing slash
        var path = relative.AsSpan(0, pathEnd).TrimEnd('/');
        var hash = FnvHash(path);

        if (includeQuery && queryIndex >= 0)
        {
            var queryEnd = fragmentIndex >= 0 ? fragmentIndex : relative.Length;
            hash = FnvHash(relative.AsSpan(queryIndex, queryEnd - queryIndex), hash);
        }

        if (includeFragment && fragmentIndex >= 0)
        {
            hash = FnvHash(relative.AsSpan(fragmentIndex), hash);
        }

        return hash.ToString("x8");
    }

    /// <summary>
    /// Computes the FNV-1a 32-bit hash of the given text, lowercasing each character for
    /// case-insensitive hashing. An optional seed allows chaining multiple segments.
    /// </summary>
    private static uint FnvHash(ReadOnlySpan<char> text, uint seed = FnvOffsetBasis)
    {
        var hash = seed;
        foreach (var c in text)
        {
            hash ^= char.ToLowerInvariant(c);
            hash *= FnvPrime;
        }
        return hash;
    }
}
