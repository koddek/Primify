namespace Primify.Converters;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

/// <summary>
/// A reflection-free JSON + BSON converter for wrapper types using Newtonsoft.Json.
/// </summary>
public sealed class NewtonsoftJsonConverter<TWrapper, TValue> : JsonConverter
    where TWrapper : IPrimify<TWrapper, TValue>
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TWrapper);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        // Cast to the wrapper type and access .Value directly. No reflection needed.
        var wrapper = (TWrapper)value;
        var innerValue = (object)wrapper.Value;

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
        return TWrapper.From((TValue)rawValue);
    }
}
