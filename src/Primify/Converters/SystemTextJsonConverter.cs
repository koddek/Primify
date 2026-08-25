namespace Primify.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// A reflection-free JSON converter for Primify wrapper types using System.Text.Json.
/// The generator attaches it to every wrapper via
/// <c>[JsonConverter(typeof(SystemTextJsonConverter&lt;TWrapper,TValue&gt;))]</c>.
/// </summary>
/// <remarks>
/// Reads deserialize the inner value first, then rebuild the wrapper through
/// <see cref="IPrimify{TSelf, TValue}.From(TValue)"/> — so invalid payloads throw the wrapper's
/// validation exception instead of producing an unvalidated instance. JSON <c>null</c> is handled
/// by System.Text.Json itself and never reaches <see cref="Read"/>.
/// </remarks>
/// <typeparam name="TWrapper">The generated wrapper type.</typeparam>
/// <typeparam name="TValue">The underlying primitive type.</typeparam>
public sealed class SystemTextJsonConverter<TWrapper, TValue> : JsonConverter<TWrapper>
    where TWrapper : IPrimify<TWrapper, TValue>
{
    /// <summary>
    /// Reads the bare primitive from the current JSON token and rebuilds a validated wrapper.
    /// </summary>
    /// <param name="reader">The JSON reader positioned at the value to convert.</param>
    /// <param name="typeToConvert">The wrapper type being deserialized.</param>
    /// <param name="options">The serializer options used for the inner value.</param>
    /// <returns>A wrapper instance rebuilt through <see cref="IPrimify{TSelf, TValue}.From(TValue)"/>.</returns>
    /// <exception cref="ArgumentException">
    /// The payload held a value that the wrapper's <c>Validate</c> hook rejected.
    /// </exception>
    public override TWrapper? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var innerValue = JsonSerializer.Deserialize<TValue>(ref reader, options);

        return TWrapper.From(innerValue!);
    }

    /// <summary>
    /// Writes only the wrapped primitive, keeping payloads flat.
    /// </summary>
    /// <param name="writer">The writer to serialize to.</param>
    /// <param name="value">The wrapper whose underlying value is written.</param>
    /// <param name="options">The serializer options forwarded for the inner value.</param>
    public override void Write(Utf8JsonWriter writer, TWrapper value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
