namespace Primify.Generator.Tests.Common;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Primify.Generator.Tests.Types;

public class SerializationBoundaryTests
{
    [Test]
    public async Task NewtonsoftJson_PreservesDateTimeOffset()
    {
        var expected = new DateTimeOffset(2026, 8, 25, 13, 45, 59, TimeSpan.FromHours(5.5));
        var wrapper = DateTimeOffsetValue.From(expected);

        var json = JsonConvert.SerializeObject(wrapper);
        var actual = JsonConvert.DeserializeObject<DateTimeOffsetValue>(json);

        await Assert.That(actual!.Value).IsEqualTo(expected);
        await Assert.That(actual.Value.Offset).IsEqualTo(expected.Offset);
    }

    [Test]
    public async Task BsonWrapper_RoundTrips()
    {
        using var stream = new MemoryStream();
        var writer = new BsonDataWriter(stream);
        var writerSerializer = new JsonSerializer();
        writerSerializer.Serialize(writer, IntStruct.From(7));
        writer.Flush();

        stream.Position = 0;
        using var reader = new BsonDataReader(stream);
        reader.Read();
        var serializer = new JsonSerializer();

        var actual = serializer.Deserialize<IntStruct>(reader);

        await Assert.That(actual.Value).IsEqualTo(7);
    }

    [Test]
    public async Task BsonDateTimeOffset_PreservesOffset()
    {
        var expected = new DateTimeOffset(2026, 8, 25, 13, 45, 59, TimeSpan.FromHours(5.5));
        using var stream = new MemoryStream();
        var writer = new BsonDataWriter(stream);
        var writerSerializer = new JsonSerializer();
        writerSerializer.Serialize(writer, DateTimeOffsetValue.From(expected));
        writer.Flush();

        stream.Position = 0;
        using var reader = new BsonDataReader(stream);
        reader.Read();
        var serializer = new JsonSerializer();

        var actual = serializer.Deserialize<DateTimeOffsetValue>(reader);

        await Assert.That(actual!.Value).IsEqualTo(expected);
        await Assert.That(actual.Value.Offset).IsEqualTo(expected.Offset);
    }

    [Test]
    public async Task MalformedBsonWrapper_IsRejected()
    {
        using var stream = new MemoryStream();
        var writer = new BsonDataWriter(stream);
        writer.WriteStartObject();
        writer.WritePropertyName("Wrong");
        writer.WriteValue(7);
        writer.WriteEndObject();
        writer.Flush();

        stream.Position = 0;
        using var reader = new BsonDataReader(stream);
        reader.Read();
        var serializer = new JsonSerializer();

        await Assert.ThrowsAsync<JsonSerializationException>(async () =>
        {
            _ = serializer.Deserialize<IntStruct>(reader);
        });
    }
}
