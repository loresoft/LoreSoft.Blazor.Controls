namespace LoreSoft.Blazor.Controls.Utilities;

/// <summary>
/// Provides simple XOR-based encryption and decryption extension methods for strings.
/// </summary>
/// <remarks>
/// <para>
/// This class provides basic XOR encryption for obfuscating string data. The encryption is symmetric,
/// meaning the same key is used for both encryption and decryption.
/// </para>
/// <para>
/// <strong>Security Warning:</strong> XOR encryption is NOT cryptographically secure and should NOT be used
/// for protecting sensitive data or in security-critical scenarios. It provides only basic obfuscation
/// and can be easily broken. Use this only for non-sensitive data where simple obfuscation is acceptable.
/// </para>
/// </remarks>
public static class XorEncryption
{
    /// <summary>
    /// Encrypts the specified text using XOR encryption with the provided key.
    /// </summary>
    /// <param name="text">The plain text string to encrypt.</param>
    /// <param name="key">The encryption key. Must not be null or empty.</param>
    /// <returns>A Base64-encoded string containing the encrypted data.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="text"/> or <paramref name="key"/> is null or empty.</exception>
    /// <remarks>
    /// The method performs XOR encryption by applying the key cyclically across the text characters.
    /// The result is Base64-encoded for safe string representation and storage.
    /// </remarks>
    public static string Encrypt(this string text, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var result = new byte[text.Length];

        for (int i = 0; i < text.Length; i++)
            result[i] = (byte)(text[i] ^ key[i % key.Length]);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts the specified encrypted text using XOR decryption with the provided key.
    /// </summary>
    /// <param name="encrypted">The Base64-encoded encrypted string to decrypt.</param>
    /// <param name="key">The decryption key. Must be the same key used for encryption and not null or empty.</param>
    /// <returns>The decrypted plain text string.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="encrypted"/> or <paramref name="key"/> is null or empty.</exception>
    /// <exception cref="FormatException">Thrown if <paramref name="encrypted"/> is not a valid Base64 string.</exception>
    /// <remarks>
    /// The method decrypts XOR-encrypted data by applying the same key cyclically across the encrypted bytes.
    /// The encrypted string must be a valid Base64-encoded string produced by the <see cref="Encrypt"/> method.
    /// </remarks>
    public static string Decrypt(this string encrypted, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(encrypted);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var bytes = Convert.FromBase64String(encrypted);
        var result = new char[bytes.Length];

        for (int i = 0; i < bytes.Length; i++)
            result[i] = (char)(bytes[i] ^ key[i % key.Length]);

        return new string(result);
    }
}

