namespace Primify.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A reflection-free JSON converter for wrapper types using System.Text.Json.
/// </summary>
public sealed class SystemTextJsonConverter<TWrapper, TValue> : JsonConverter<TWrapper>
    where TWrapper : IPrimify<TWrapper, TValue>
{
    public override TWrapper? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var innerValue = JsonSerializer.Deserialize<TValue>(ref reader, options);

        return TWrapper.From(innerValue!);
    }

    public override void Write(Utf8JsonWriter writer, TWrapper value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
