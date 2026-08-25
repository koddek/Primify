namespace Primify.Converters;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

/// <summary>
/// A reflection-free JSON + BSON converter for Primify wrapper types using Newtonsoft.Json.
/// The generator attaches it to every wrapper via
/// <c>[JsonConverter(typeof(NewtonsoftJsonConverter&lt;TWrapper,TValue&gt;))]</c>.
/// </summary>
/// <remarks>
/// <para>
/// Standard JSON payloads carry the bare primitive. When writing to a
/// <see cref="BsonDataWriter"/>, the value is wrapped as <c>{ "Value": ... }</c> so BSON keeps a
/// document shape.
/// </para>
/// <para>
/// Deserialization rebuilds the wrapper through <see cref="IPrimify{TSelf, TValue}.From(TValue)"/> —
/// invalid payloads throw the wrapper's validation exception instead of yielding an unvalidated
/// instance.
/// </para>
/// </remarks>
/// <typeparam name="TWrapper">The generated wrapper type.</typeparam>
/// <typeparam name="TValue">The underlying primitive type.</typeparam>
public sealed class NewtonsoftJsonConverter<TWrapper, TValue> : JsonConverter
    where TWrapper : IPrimify<TWrapper, TValue>
{
    /// <summary>Determines whether this converter handles the given type (exact wrapper match only).</summary>
    /// <param name="objectType">The type to convert.</param>
    /// <returns>true when <paramref name="objectType"/> is exactly <typeparamref name="TWrapper"/>.</returns>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TWrapper);
    }

    /// <summary>Writes the wrapped primitive; wraps in <c>{ "Value": ... }</c> for BSON writers.</summary>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        // Cast to the wrapper type and access .Value directly. No reflection needed.
        var wrapper = (TWrapper)value;
        var innerValue = (object)wrapper.Value!;

        if (innerValue is DateTimeOffset dto)
        {
            innerValue = dto.UtcDateTime;
        }

        if (writer is BsonDataWriter)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Value");
            serializer.Serialize(writer, innerValue);
            writer.WriteEndObject();
        }
        else
        {
            serializer.Serialize(writer, innerValue);
        }
    }

    /// <summary>Reads the primitive (or BSON <c>{ "Value": ... }</c> shape) and returns a validated wrapper.</summary>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        // Handle the BSON DateTimeOffset issue as before
        if (reader is BsonDataReader bsonReader && typeof(TValue) == typeof(DateTimeOffset))
        {
            var originalKind = bsonReader.DateTimeKindHandling;
            try
            {
                bsonReader.DateTimeKindHandling = DateTimeKind.Utc;
                return ReadAndWrapValue(reader, serializer);
            }
            finally
            {
                bsonReader.DateTimeKindHandling = originalKind;
            }
        }

        return ReadAndWrapValue(reader, serializer);
    }

    private object? ReadAndWrapValue(JsonReader reader, JsonSerializer serializer)
    {
        object? rawValue;

        if (reader.TokenType == JsonToken.StartObject)
        {
            // BSON path: { "Value": <...> }
            reader.Read();
            reader.Read(); // Advance to the value
            rawValue = serializer.Deserialize<TValue>(reader);
            reader.Read(); // Consume EndObject
        }
        else
        {
            // Standard JSON path
            rawValue = serializer.Deserialize<TValue>(reader);
        }

        // No reflection! Call the static 'From' method directly.
        return TWrapper.From((TValue)rawValue!);
    }
}
