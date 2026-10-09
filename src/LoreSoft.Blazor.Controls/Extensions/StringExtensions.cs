#nullable enable

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LoreSoft.Blazor.Controls.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="string"/> values, including truncation, combination, and formatting.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Truncates the specified string to a maximum length, optionally appending an ellipsis or custom suffix if truncation occurs.
    /// </summary>
    /// <param name="text">The string to truncate.</param>
    /// <param name="keep">The number of characters to keep (including the ellipsis, if used).</param>
    /// <param name="ellipsis">The string to append if truncation occurs. Defaults to "..." if not specified.</param>
    /// <returns>
    /// The truncated string with the ellipsis (or custom suffix) appended if truncation occurred; otherwise, the original string.
    /// Returns <see langword="null"/> if <paramref name="text"/> is <see langword="null"/>.
    /// </returns>
    [return: NotNullIfNotNull(nameof(text))]
    public static string? Truncate(this string? text, int keep, string? ellipsis = "...")
    {
        if (string.IsNullOrEmpty(text) || text.Length <= keep)
            return text;

        ellipsis ??= string.Empty;

        int ellipsisLength = ellipsis.Length;

        // If there's no room for ellipsis, just return truncated prefix
        if (keep <= ellipsisLength)
            return text[..keep];

        int prefixLength = keep - ellipsisLength;
        int totalLength = prefixLength + ellipsisLength;

        // Use stack allocation for short strings, or rent from the pool for longer ones
        char[]? rentedArray = null;
        Span<char> buffer = totalLength <= 256
            ? stackalloc char[totalLength]
            : (rentedArray = ArrayPool<char>.Shared.Rent(totalLength));

        try
        {
            text.AsSpan(0, prefixLength).CopyTo(buffer);
            ellipsis.AsSpan().CopyTo(buffer[prefixLength..]);

            return new string(buffer[..totalLength]);
        }
        finally
        {
            // Return rented array to the pool to avoid memory leaks
            if (rentedArray != null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    /// <summary>
    /// Determines whether the specified string is <see langword="null"/> or an empty string.
    /// </summary>
    /// <param name="value">A string reference.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> is <see langword="null"/> or empty; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Determines whether the specified string is <see langword="null"/>, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">A string reference.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> is <see langword="null"/>, empty, or whitespace; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Determines whether the specified string is not <see cref="IsNullOrEmpty"/>.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> is not <see langword="null"/> or empty; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool HasValue([NotNullWhen(true)] this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Combines two strings with the specified separator, ensuring only a single separator character is present between them.
    /// </summary>
    /// <param name="first">The first string.</param>
    /// <param name="second">The second string.</param>
    /// <param name="separator">The separator character to use. Defaults to '/'.</param>
    /// <returns>
    /// A string combining <paramref name="first"/> and <paramref name="second"/> with the <paramref name="separator"/> between them.
    /// If either string is <see langword="null"/> or empty, returns the other string.
    /// </returns>
    [return: NotNullIfNotNull(nameof(first))]
    [return: NotNullIfNotNull(nameof(second))]
    public static string? Combine(this string? first, string? second, char separator = '/')
    {
        if (string.IsNullOrEmpty(first))
            return second;

        if (string.IsNullOrEmpty(second))
            return first;

        bool firstEndsWith = first![^1] == separator;
        bool secondStartsWith = second![0] == separator;

        int firstLength = first.Length;
        int secondLength = second.Length;

        // XOR operator returns true if exactly one of its operands is true, but not both and not neither.
        if (firstEndsWith ^ secondStartsWith)
        {
            // No separator adjustment needed
            var totalLength = firstLength + secondLength;
            return string.Create(totalLength, (first, second), static (span, state) =>
            {
                state.first.AsSpan().CopyTo(span);
                state.second.AsSpan().CopyTo(span[state.first.Length..]);
            });
        }

        if (firstEndsWith && secondStartsWith)
        {
            // Remove one separator to avoid duplication
            var totalLength = firstLength + secondLength - 1;
            return string.Create(totalLength, (first, second), static (span, state) =>
            {
                state.first.AsSpan().CopyTo(span);
                state.second.AsSpan(1).CopyTo(span[state.first.Length..]);
            });
        }

        // Need to insert a separator
        var total = firstLength + 1 + secondLength;
        return string.Create(total, (first, second, separator), static (span, state) =>
        {
            state.first.AsSpan().CopyTo(span);
            span[state.first.Length] = state.separator;
            state.second.AsSpan().CopyTo(span[(state.first.Length + 1)..]);
        });
    }

    /// <summary>
    /// Converts a string to a human-readable title case, inserting spaces at word boundaries.
    /// Word boundaries are detected at transitions between lowercase and uppercase letters, between letters and digits, and at non-alphanumeric characters.
    /// Non-alphanumeric characters are treated as word separators and are replaced with spaces.
    /// The first letter of each word is capitalized.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>
    /// A title-cased string with spaces inserted at word boundaries, or <see langword="null"/> if <paramref name="input"/> is <see langword="null"/>.
    /// If <paramref name="input"/> is empty, returns an empty string.
    /// </returns>
    [return: NotNullIfNotNull(nameof(input))]
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "Optimized Method")]
    public static string? ToTitle(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        ReadOnlySpan<char> span = input.AsSpan();

        // Estimate output size with padding for added spaces
        int estimatedSize = span.Length + (span.Length / 4);

        // Use stack allocation for short strings, or rent from the pool for longer ones
        char[]? rentedArray = null;
        Span<char> buffer = estimatedSize <= 256
            ? stackalloc char[estimatedSize]
            : (rentedArray = ArrayPool<char>.Shared.Rent(estimatedSize));

        try
        {
            int j = 0;                 // Buffer write index
            bool atWordStart = true;   // Track if next character starts a new word

            for (int i = 0; i < span.Length; i++)
            {
                char current = span[i];

                // Treat any non-alphanumeric character as a word separator (convert to space)
                if (!char.IsLetterOrDigit(current))
                {
                    if (j > 0 && buffer[j - 1] != ' ')
                    {
                        buffer[j++] = ' ';
                        atWordStart = true;
                    }
                    continue;
                }

                if (i > 0)
                {
                    char prev = span[i - 1];

                    bool isUpper = char.IsUpper(current);
                    bool wasLower = char.IsLower(prev);
                    bool wasUpper = char.IsUpper(prev);
                    bool isDigit = char.IsDigit(current);
                    bool wasLetter = char.IsLetter(prev);
                    bool wasDigit = char.IsDigit(prev);

                    // Determine if a space should be inserted before the current character
                    bool insertSpace =
                        // Case 1: lowercase letter followed by uppercase (camelCase transition) "firstName" → "First Name"
                        (isUpper && wasLower) ||
                        // Case 2: consecutive uppercase letters followed by a lowercase letter (e.g., "PDFName" → "PDF Name")
                        (isUpper && wasUpper && i + 1 < span.Length && char.IsLower(span[i + 1])) ||
                        // Case 3: letter followed by digit (e.g., "Version2" → "Version 2")
                        (isDigit && wasLetter) ||
                        // Case 4: digit followed by letter (e.g., "IP4Address" → "IP 4 Address")
                        (char.IsLetter(current) && wasDigit);

                    if (insertSpace)
                    {
                        buffer[j++] = ' ';
                        atWordStart = true;
                    }
                }

                if (atWordStart)
                {
                    // Capitalize the first letter of each word
                    buffer[j++] = char.ToUpperInvariant(current);
                    atWordStart = false;
                }
                else
                {
                    buffer[j++] = current;
                }
            }

            return new string(buffer[..j]);
        }
        finally
        {
            // Return rented array to the pool to avoid memory leaks
            if (rentedArray != null)
                ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    /// <summary>
    /// Masks a string by replacing characters with a specified mask character,
    /// preserving a certain number of characters at the start and end.
    /// Optionally, you can specify a fixed number of masked characters in the middle.
    /// </summary>
    /// <param name="input">The input string to mask.</param>
    /// <param name="unmaskedStart">Number of characters to leave unmasked at the start.</param>
    /// <param name="unmaskedEnd">Number of characters to leave unmasked at the end.</param>
    /// <param name="maskChar">The character to use for masking. Default is '*'.</param>
    /// <param name="maskedCount">
    /// Optional. If specified, the number of mask characters to use in the masked section.
    /// If not specified, the masked section will fill the space between the unmasked start and end.
    /// </param>
    /// <returns>The masked string.</returns>
    /// <example>
    /// <code>
    /// mask credit card number
    /// string maskedCreditCard = "4111111111111111".Mask(4, 4);
    /// Assert.Equal("4111********1111", maskedCreditCard);
    ///
    /// // mask a social security number
    /// var maskedSocial = "123-45-6789".Mask(0, 4, '*', 5);
    /// Assert.Equal("*****6789", maskedSocial);
    ///
    /// // mask a password
    /// var maskedPassword = "P@ssw0rd123".Mask(0, 0, '*', 8);
    /// Assert.Equal("********", maskedPassword);
    /// </code>
    /// </example>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? Mask(
        this string? input,
        int unmaskedStart,
        int unmaskedEnd,
        char maskChar = '*',
        int? maskedCount = null)
    {
        if (input.IsNullOrEmpty())
            return input;

        int length = input.Length;

        if (unmaskedStart + unmaskedEnd > length)
            throw new ArgumentOutOfRangeException(nameof(input), "Unmasked prefix and suffix exceed input length.");

        if (length == 0 || (unmaskedStart == 0 && unmaskedEnd == 0 && maskChar == '\0'))
            return string.Empty;

        int maskedSectionLength = maskedCount ?? (length - unmaskedStart - unmaskedEnd);
        if (maskedSectionLength < 0)
            maskedSectionLength = 0;

        int totalLength = unmaskedStart + maskedSectionLength + unmaskedEnd;

        Span<char> buffer = totalLength <= 256
            ? stackalloc char[totalLength]
            : ArrayPool<char>.Shared.Rent(totalLength);

        try
        {
            int pos = 0;

            // Copy prefix
            if (unmaskedStart > 0)
            {
                input.AsSpan(0, unmaskedStart).CopyTo(buffer.Slice(pos, unmaskedStart));
                pos += unmaskedStart;
            }

            // Fill middle with maskChar
            if (maskedSectionLength > 0)
            {
                buffer.Slice(pos, maskedSectionLength).Fill(maskChar);
                pos += maskedSectionLength;
            }

            // Copy suffix
            if (unmaskedEnd > 0)
            {
                input.AsSpan(length - unmaskedEnd, unmaskedEnd).CopyTo(buffer.Slice(pos, unmaskedEnd));
                pos += unmaskedEnd;
            }

            return new string(buffer[..totalLength]);
        }
        finally
        {
            if (totalLength > 256)
                ArrayPool<char>.Shared.Return(buffer.ToArray(), clearArray: true);
        }
    }

    /// <summary>
    /// Converts a string to a <see cref="Uri"/> object. If the string is not a well-formed
    /// absolute URI, it prepends the specified default scheme (e.g., "https") to the string
    /// before creating the <see cref="Uri"/>.
    /// </summary>
    /// <param name="baseUri">The input string to be converted to a <see cref="Uri"/>.</param>
    /// <param name="defaultScheme">The default scheme to prepend if the input string is not a well-formed absolute URI. Defaults to "https".</param>
    /// <returns>A <see cref="Uri"/> object representing the input string, or null if the input string is null or empty.</returns>
    [return: NotNullIfNotNull(nameof(baseUri))]
    public static Uri? ToUri(
        this string? baseUri,
        string defaultScheme = "https")
    {
        if (string.IsNullOrEmpty(baseUri))
            return null;

        if (Uri.IsWellFormedUriString(baseUri, UriKind.Absolute))
            return new Uri(baseUri);

        return new Uri(defaultScheme + Uri.SchemeDelimiter + baseUri, UriKind.Absolute);
    }

    /// <summary>
    /// Computes the SHA-256 hash of the input string and returns it as a hexadecimal string.
    /// </summary>
    /// <param name="value">The input string to hash.</param>
    /// <returns>
    /// A hexadecimal string representation of the SHA-256 hash, or an empty string if <paramref name="value"/> is <c>null</c> or empty.
    /// </returns>
    [return: NotNullIfNotNull(nameof(value))]
    public static string? ToSha256(this string? value)
    {
        if (value.IsNullOrEmpty())
            return null;

        // Calculate the max bytes needed for UTF-8 encoding
        int maxByteCount = Encoding.UTF8.GetMaxByteCount(value.Length);

        // Use stack allocation for small strings, or rent from the pool for larger ones
        byte[]? rentedByteArray = null;

        Span<byte> bytesBuffer = maxByteCount <= 512
            ? stackalloc byte[maxByteCount]
            : (rentedByteArray = ArrayPool<byte>.Shared.Rent(maxByteCount));

        // SHA-256 produces a 32-byte hash
        Span<byte> hashBuffer = stackalloc byte[32];

        try
        {
            // Encode the string to UTF-8 bytes
            int actualByteCount = Encoding.UTF8.GetBytes(value.AsSpan(), bytesBuffer);

            // Compute the SHA-256 hash
            System.Security.Cryptography.SHA256.HashData(bytesBuffer[..actualByteCount], hashBuffer);

            // Convert to hex string
            return Convert.ToHexString(hashBuffer);
        }
        finally
        {
            // Return rented arrays to the pool to avoid memory leaks
            if (rentedByteArray != null)
                ArrayPool<byte>.Shared.Return(rentedByteArray);
        }
    }
}
