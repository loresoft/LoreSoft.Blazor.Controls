using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace LoreSoft.Blazor.Controls;

public class StorageService
{
    public const string LocalStorage = "localStorage";
    public const string SessionStorage = "sessionStorage";

    private readonly IJSRuntime _javaScript;
    private readonly JsonSerializerOptions _options;
    private readonly ILogger<StorageService> _logger;

    public StorageService(
        IJSRuntime javaScript,
        IOptions<JsonSerializerOptions> options,
        ILogger<StorageService> logger)
    {
        _javaScript = javaScript;
        _options = options.Value;
        _logger = logger;
    }


    public async ValueTask<T?> GetItemAsync<T>(string key, StoreType storeType = StoreType.Session)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var module = storeType == StoreType.Local ? LocalStorage : SessionStorage;
        var method = $"{module}.getItem";

        var json = await _javaScript.InvokeAsync<string?>(method, key);
        if (string.IsNullOrWhiteSpace(json))
            return default;

        _logger.LogInformation("Retrieved {StoreType} storage item {Key} with value {Value}", storeType, key, json);

        return JsonSerializer.Deserialize<T>(json, _options);
    }

    public async ValueTask SetItemAsync<T>(string key, T value, StoreType storeType = StoreType.Session)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var module = storeType == StoreType.Local ? LocalStorage : SessionStorage;
        var method = $"{module}.setItem";

        var json = value is null ? string.Empty : JsonSerializer.Serialize(value, _options);

        _logger.LogInformation("Setting {StoreType} storage item {Key} to {Value}", storeType, key, json);

        await _javaScript.InvokeVoidAsync(method, key, json);
    }

    public async ValueTask RemoveItemAsync(string key, StoreType storeType = StoreType.Session)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var module = storeType == StoreType.Local ? LocalStorage : SessionStorage;
        var method = $"{module}.removeItem";

        await _javaScript.InvokeVoidAsync(method, key);
    }

    public async ValueTask ClearAsync(StoreType storeType = StoreType.Session)
    {
        var module = storeType == StoreType.Local ? LocalStorage : SessionStorage;
        var method = $"{module}.clear";

        await _javaScript.InvokeVoidAsync(method);
    }
}

public enum StoreType
{
    Local,
    Session
}

