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
/// Standard JSON payloads carry the bare primitive. <see cref="DateTimeOffset"/>
/// values use <c>{ "UtcTicks": ..., "OffsetTicks": ... }</c> so the original offset survives
/// Json.NET's date tokenization. When writing to a <see cref="BsonDataWriter"/>, the value is
/// wrapped as <c>{ "Value": ... }</c>; <see cref="DateTimeOffset"/> uses the same tick object
/// inside that field.
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

        if (innerValue is DateTimeOffset dateTimeOffset && writer is not BsonDataWriter)
        {
            WriteDateTimeOffsetObject(writer, dateTimeOffset);
        }
        else if (writer is BsonDataWriter)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Value");
            if (innerValue is DateTimeOffset dto)
            {
                WriteDateTimeOffsetObject(writer, dto);
            }
            else
            {
                serializer.Serialize(writer, innerValue);
            }

            writer.WriteEndObject();
        }
        else
        {
            serializer.Serialize(writer, innerValue);
        }
    }

    /// <summary>Reads a primitive, a DateTimeOffset tick object, or a BSON <c>{ "Value": ... }</c> shape and returns a validated wrapper.</summary>
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
            if (typeof(TValue) == typeof(DateTimeOffset) && reader is JsonTextReader)
            {
                rawValue = ReadDateTimeOffsetObject(reader);
            }
            else if (reader is not BsonDataReader)
            {
                throw new JsonSerializationException("A Primify wrapper must be a primitive JSON value.");
            }
            else
            {
                if (!reader.Read() ||
                    reader.TokenType != JsonToken.PropertyName ||
                    !string.Equals(reader.Value as string, "Value", StringComparison.Ordinal))
                {
                    throw new JsonSerializationException("The BSON wrapper object must contain a Value property.");
                }

                if (!reader.Read())
                {
                    throw new JsonSerializationException("The BSON wrapper object has no value.");
                }

                rawValue = typeof(TValue) == typeof(DateTimeOffset) && reader.TokenType == JsonToken.StartObject
                    ? ReadDateTimeOffsetObject(reader)
                    : serializer.Deserialize<TValue>(reader);

                if (!reader.Read() || reader.TokenType != JsonToken.EndObject)
                {
                    throw new JsonSerializationException("The BSON wrapper object must contain exactly one Value property.");
                }
            }
        }
        else
        {
            // Standard JSON path
            rawValue = serializer.Deserialize<TValue>(reader);
        }

        // No reflection! Call the static 'From' method directly.
        return TWrapper.From((TValue)rawValue!);
    }

    private static void WriteDateTimeOffsetObject(JsonWriter writer, DateTimeOffset value)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("UtcTicks");
        writer.WriteValue(value.UtcTicks);
        writer.WritePropertyName("OffsetTicks");
        writer.WriteValue(value.Offset.Ticks);
        writer.WriteEndObject();
    }

    private static DateTimeOffset ReadDateTimeOffsetObject(JsonReader reader)
    {
        if (!reader.Read() ||
            reader.TokenType != JsonToken.PropertyName ||
            !string.Equals(reader.Value as string, "UtcTicks", StringComparison.Ordinal) ||
            !reader.Read() ||
            reader.TokenType != JsonToken.Integer)
        {
            throw new JsonSerializationException("A DateTimeOffset JSON object must start with UtcTicks.");
        }

        var utcTicks = Convert.ToInt64(reader.Value, System.Globalization.CultureInfo.InvariantCulture);

        if (!reader.Read() ||
            reader.TokenType != JsonToken.PropertyName ||
            !string.Equals(reader.Value as string, "OffsetTicks", StringComparison.Ordinal) ||
            !reader.Read() ||
            reader.TokenType != JsonToken.Integer)
        {
            throw new JsonSerializationException("A DateTimeOffset JSON object must contain OffsetTicks.");
        }

        var offsetTicks = Convert.ToInt64(reader.Value, System.Globalization.CultureInfo.InvariantCulture);

        if (!reader.Read() || reader.TokenType != JsonToken.EndObject)
        {
            throw new JsonSerializationException("A DateTimeOffset JSON object must end after OffsetTicks.");
        }

        try
        {
            return new DateTimeOffset(utcTicks, TimeSpan.Zero)
                .ToOffset(TimeSpan.FromTicks(offsetTicks));
        }
        catch (ArgumentException exception)
        {
            throw new JsonSerializationException("The DateTimeOffset JSON object contains invalid ticks.", exception);
        }
    }
}
