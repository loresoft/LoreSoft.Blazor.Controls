using System.Text.Json;
using System.Text.Json.Serialization;

using LoreSoft.Blazor.Controls.Extensions;

namespace LoreSoft.Blazor.Controls.Converters;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for <see cref="object"/> that preserves the exact CLR type
/// by serializing values as a two-element JSON array: <c>["TypeName", value]</c>.
/// On read, the type name is resolved first, then the value is deserialized to that exact type,
/// eliminating all ambiguity about which CLR type the value should map to.
/// Null values are written and read as JSON <c>null</c>.
/// </summary>
/// <remarks>
/// The first array element is the assembly-qualified portable type name produced by
/// <c>GetPortableName()</c>, and the second element is the JSON-serialized value.
/// This converter is intended for use with polymorphic <see cref="object"/> properties
/// where the concrete type must survive a round-trip.
/// </remarks>
internal sealed class JsonObjectConverter : JsonConverter<object?>
{
    /// <summary>
    /// Reads and converts JSON into an <see cref="object"/> by first resolving the CLR type
    /// from the type-name string stored as the first element of the two-element JSON array,
    /// then deserializing the second element to that type.
    /// Returns <see langword="null"/> when the JSON token is <c>null</c>.
    /// </summary>
    /// <param name="reader">The reader to read JSON from.</param>
    /// <param name="typeToConvert">The target type (always <see cref="object"/>).</param>
    /// <param name="options">The serializer options to use when deserializing the value element.</param>
    /// <returns>
    /// The deserialized CLR value, or <see langword="null"/> if the JSON token is <c>null</c>.
    /// </returns>
    /// <exception cref="JsonException">
    /// Thrown when the JSON structure is not a two-element array, the type-name element is missing
    /// or not a string, or the type name cannot be resolved to a CLR type.
    /// </exception>
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($"Expected start of array or null, got {reader.TokenType}.");

        if (!reader.Read() || reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected type name string as first array element.");

        var typeName = reader.GetString()!;
        var type = Type.GetType(typeName, throwOnError: false)
            ?? throw new JsonException($"Cannot resolve type '{typeName}'.");

        reader.Read();
        var value = JsonSerializer.Deserialize(ref reader, type, options);

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected end of array after value element.");

        return value;
    }

    /// <summary>
    /// Writes an <see cref="object"/> value as a two-element JSON array
    /// <c>["TypeFullName", value]</c>, where the first element is the portable assembly-qualified
    /// type name and the second element is the JSON-serialized value.
    /// Writes JSON <c>null</c> when <paramref name="value"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="writer">The writer to write JSON to.</param>
    /// <param name="value">The value to serialize. May be <see langword="null"/>.</param>
    /// <param name="options">The serializer options to use when serializing the value element.</param>
    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var type = value.GetType();

        writer.WriteStartArray();

        writer.WriteStringValue(type.GetPortableName());
        JsonSerializer.Serialize(writer, value, type, options);

        writer.WriteEndArray();
    }
}
