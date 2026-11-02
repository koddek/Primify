namespace Primify.Generator.Tests.Common;

using LiteDB;
using Primify.Converters;
using Primify.Generator.Tests.Types;

public class LiteDbRegistrationTests
{
    [Test]
    public async Task Register_AddsBuiltInMappings_OnCustomMapper()
    {
        var mapper = new BsonMapper();

        LiteDbRegistration.Register(mapper);

        var serialized = mapper.Serialize(IntRecordStruct.From(-42));
        await Assert.That(serialized.AsInt32).IsEqualTo(-42);

        var actual = mapper.Deserialize<IntRecordStruct>(serialized);
        await Assert.That(actual).IsEqualTo(IntRecordStruct.From(-42));
    }

    [Test]
    public async Task UnsupportedWrapper_IsSkipped()
    {
        var mapper = new BsonMapper();

        LiteDbRegistration.Register(mapper);

        // No Primify mapping was registered for the unsupported wrapper, so LiteDB falls back to its
        // default POCO shape instead of a primitive bridge.
        var serialized = mapper.Serialize(UnsupportedLiteDbWrapper.From(new UnsupportedLiteDbValue("value")));
        await Assert.That(serialized.IsDocument).IsTrue();
    }

    [Test]
    public async Task UnsupportedWrapper_CanBeMappedExplicitly()
    {
        var mapper = new BsonMapper();
        mapper.RegisterType<UnsupportedLiteDbWrapper>(
            serialize: wrapper => new BsonValue(wrapper.Value.Text),
            deserialize: value => UnsupportedLiteDbWrapper.From(new UnsupportedLiteDbValue(value.AsString)));
        var expected = UnsupportedLiteDbWrapper.From(new UnsupportedLiteDbValue("value"));

        var serialized = mapper.Serialize(expected);
        var actual = mapper.Deserialize<UnsupportedLiteDbWrapper>(serialized);

        await Assert.That(actual.Value.Text).IsEqualTo(expected.Value.Text);
    }
}
