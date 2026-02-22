using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using LoreSoft.Blazor.Controls.Extensions;
using LoreSoft.Blazor.Controls.Utilities;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

/// <summary>
/// Provides methods for interacting with browser local storage and session storage.
/// </summary>
/// <remarks>
/// <para>
/// This service provides a wrapper around the browser's localStorage and sessionStorage APIs,
/// with support for JSON serialization/deserialization and optional XOR-based obfuscation.
/// </para>
/// <para>
/// Local storage persists data across browser sessions and has no expiration time,
/// while session storage only persists for the duration of the page session.
/// </para>
/// <para>
/// The optional <c>protectionKey</c> parameter enables basic obfuscation using XOR encryption.
/// This is NOT cryptographically secure and should only be used to prevent casual inspection
/// of stored values, not for protecting sensitive data. For sensitive data, use proper
/// encryption or store data server-side.
/// </para>
/// </remarks>
public partial class StorageService
{
    /// <summary>
    /// The JavaScript module name for local storage.
    /// </summary>
    public const string LocalStorage = "localStorage";

    /// <summary>
    /// The JavaScript module name for session storage.
    /// </summary>
    public const string SessionStorage = "sessionStorage";

    private readonly IJSRuntime _javaScript;
    private readonly JsonSerializerOptions _options;
    private readonly ILogger<StorageService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageService"/> class.
    /// </summary>
    /// <param name="javaScript">The JavaScript runtime for interop calls.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <param name="logger">The logger instance.</param>
    public StorageService(
        IJSRuntime javaScript,
        IOptions<JsonSerializerOptions> options,
        ILogger<StorageService> logger)
    {
        _javaScript = javaScript;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a string value from browser storage.
    /// </summary>
    /// <param name="key">The storage key.</param>
    /// <param name="storeType">The type of storage to use (local or session).</param>
    /// <param name="protectionKey">
    /// Optional encryption key to decrypt the stored value.
    /// NOTE: Uses XOR encryption which is NOT cryptographically secure and should only
    /// be used for obfuscation, not for protecting sensitive data.
    /// </param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the stored string value, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    public async ValueTask<string?> GetItemAsync(
        string key,
        StoreType storeType = StoreType.Session,
        string? protectionKey = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var module = storeType == StoreType.Local ? LocalStorage : SessionStorage;
        var method = $"{module}.getItem";

        string? value;

        try
        {
            value = await _javaScript.InvokeAsync<string?>(method, key);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or InvalidOperationException or TaskCanceledException)
        {
            LogGetItemUnavailable(_logger, ex, key);
            return null;
        }

        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(protectionKey))
            return value;

        return value.Decrypt(protectionKey);
    }

    /// <summary>
    /// Retrieves and deserializes a typed value from browser storage.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the stored JSON value to.</typeparam>
    /// <param name="key">The storage key.</param>
    /// <param name="storeType">The type of storage to use (local or session).</param>
    /// <param name="protectionKey">
    /// Optional encryption key to decrypt the stored value.
    /// NOTE: Uses XOR encryption which is NOT cryptographically secure and should only
    /// be used for obfuscation, not for protecting sensitive data.
    /// </param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized value, or default if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    public async ValueTask<T?> GetItemAsync<T>(
        string key,
        StoreType storeType = StoreType.Session,
        string? protectionKey = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var json = await GetItemAsync(key, storeType, protectionKey);
        if (string.IsNullOrWhiteSpace(json))
            return default;

        LogGetItem(_logger, storeType, key, json);

        return JsonSerializer.Deserialize<T>(json, _options);
    }

    /// <summary>
    /// Retrieves and deserializes a typed value from browser storage using the specified <see cref="JsonTypeInfo{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the stored JSON value to.</typeparam>
    /// <param name="key">The storage key.</param>
    /// <param name="jsonTypeInfo">The JSON type information used for deserialization.</param>
    /// <param name="storeType">The type of storage to use (local or session).</param>
    /// <param name="protectionKey">
    /// Optional encryption key to decrypt the stored value.
    /// NOTE: Uses XOR encryption which is NOT cryptographically secure and should only
    /// be used for obfuscation, not for protecting sensitive data.
    /// </param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized value, or default if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonTypeInfo"/> is null.</exception>
    public async ValueTask<T?> GetItemAsync<T>(
        string key,
        JsonTypeInfo<T> jsonTypeInfo,
        StoreType storeType = StoreType.Session,
        string? protectionKey = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        var json = await GetItemAsync(key, storeType, protectionKey);
        if (string.IsNullOrWhiteSpace(json))
            return default;

        LogGetItem(_logger, storeType, key, json);

        return JsonSerializer.Deserialize(json, jsonTypeInfo);
    }


    /// <summary>
    /// Stores a string value in browser storage.
    /// </summary>
    /// <param name="key">The storage key.</param>
    /// <param name="value">The string value to store.</param>
    /// <param name="storeType">The type of storage to use (local or session).</param>
    /// <param name="protectionKey">
    /// Optional encryption key to encrypt the stored value.
    /// NOTE: Uses XOR encryption which is NOT cryptographically secure and should only
    /// be used for obfuscation, not for protecting sensitive data.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    public async ValueTask SetItemAsync(
        string key,
        string value,
        StoreType storeType = StoreType.Session,
        string? protectionKey = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var module = storeType == StoreType.Local ? LocalStorage : SessionStorage;
        var method = $"{module}.setItem";

        try
        {
            var encrypted = value;
            if (protectionKey.HasValue())
                encrypted = encrypted.Encrypt(protectionKey!);

            await _javaScript.InvokeVoidAsync(method, key, encrypted);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or InvalidOperationException or TaskCanceledException)
        {
            LogSetItemUnavailable(_logger, ex, key);
        }
    }

    /// <summary>
    /// Serializes and stores a typed value in browser storage.
    /// </summary>
    /// <typeparam name="T">The type of value to serialize and store.</typeparam>
    /// <param name="key">The storage key.</param>
    /// <param name="value">The value to serialize and store.</param>
    /// <param name="storeType">The type of storage to use (local or session).</param>
    /// <param name="protectionKey">
    /// Optional encryption key to encrypt the stored value.
    /// NOTE: Uses XOR encryption which is NOT cryptographically secure and should only
    /// be used for obfuscation, not for protecting sensitive data.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    public async ValueTask SetItemAsync<T>(
        string key,
        T value,
        StoreType storeType = StoreType.Session,
        string? protectionKey = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var json = value is null ? string.Empty : JsonSerializer.Serialize(value, _options);

        LogSetItem(_logger, storeType, key, json);

        await SetItemAsync(key, json, storeType, protectionKey);
    }

    /// <summary>
    /// Serializes and stores a typed value in browser storage using the specified <see cref="JsonTypeInfo{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value to serialize and store.</typeparam>
    /// <param name="key">The storage key.</param>
    /// <param name="value">The value to serialize and store.</param>
    /// <param name="jsonTypeInfo">The JSON type information used for serialization.</param>
    /// <param name="storeType">The type of storage to use (local or session).</param>
    /// <param name="protectionKey">
    /// Optional encryption key to encrypt the stored value.
    /// NOTE: Uses XOR encryption which is NOT cryptographically secure and should only
    /// be used for obfuscation, not for protecting sensitive data.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonTypeInfo"/> is null.</exception>
    public async ValueTask SetItemAsync<T>(
        string key,
        T value,
        JsonTypeInfo<T> jsonTypeInfo,
        StoreType storeType = StoreType.Session,
        string? protectionKey = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        var json = value is null ? string.Empty : JsonSerializer.Serialize(value, jsonTypeInfo);

        LogSetItem(_logger, storeType, key, json);

        await SetItemAsync(key, json, storeType, protectionKey);
    }


    /// <summary>
    /// Removes an item from browser storage.
    /// </summary>
    /// <param name="key">The storage key to remove.</param>
    /// <param name="storeType">The type of storage to use (local or session).</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    public async ValueTask RemoveItemAsync(string key, StoreType storeType = StoreType.Session)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var module = storeType == StoreType.Local ? LocalStorage : SessionStorage;
        var method = $"{module}.removeItem";

        try
        {
            await _javaScript.InvokeVoidAsync(method, key);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or InvalidOperationException or TaskCanceledException)
        {
            LogRemoveItemUnavailable(_logger, ex, key);
        }
    }

    /// <summary>
    /// Clears all items from browser storage.
    /// </summary>
    /// <param name="storeType">The type of storage to clear (local or session).</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async ValueTask ClearAsync(StoreType storeType = StoreType.Session)
    {
        var module = storeType == StoreType.Local ? LocalStorage : SessionStorage;
        var method = $"{module}.clear";

        try
        {
            await _javaScript.InvokeVoidAsync(method);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or InvalidOperationException or TaskCanceledException)
        {
            LogClearUnavailable(_logger, ex, storeType);
        }
    }


    [LoggerMessage(Level = LogLevel.Debug, Message = "JavaScript runtime unavailable when getting storage item {Key}")]
    static partial void LogGetItemUnavailable(ILogger logger, Exception ex, string key);

    [LoggerMessage(Level = LogLevel.Information, Message = "Retrieved {StoreType} storage item {Key} with value {Value}")]
    static partial void LogGetItem(ILogger logger, StoreType storeType, string key, string? value);

    [LoggerMessage(Level = LogLevel.Debug, Message = "JavaScript runtime unavailable when setting storage item {Key}")]
    static partial void LogSetItemUnavailable(ILogger logger, Exception ex, string key);

    [LoggerMessage(Level = LogLevel.Information, Message = "Setting {StoreType} storage item {Key} to {Value}")]
    static partial void LogSetItem(ILogger logger, StoreType storeType, string key, string? value);

    [LoggerMessage(Level = LogLevel.Debug, Message = "JavaScript runtime unavailable when removing storage item {Key}")]
    static partial void LogRemoveItemUnavailable(ILogger logger, Exception ex, string key);

    [LoggerMessage(Level = LogLevel.Debug, Message = "JavaScript runtime unavailable when clearing {StoreType} storage")]
    static partial void LogClearUnavailable(ILogger logger, Exception ex, StoreType storeType);
}

/// <summary>
/// Specifies the type of browser storage to use.
/// </summary>
public enum StoreType
{
    /// <summary>
    /// Local storage persists data across browser sessions.
    /// </summary>
    Local,

    /// <summary>
    /// Session storage persists data only for the current browser session.
    /// </summary>
    Session
}

