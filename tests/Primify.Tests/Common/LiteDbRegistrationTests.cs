namespace Primify.Generator.Tests.Common;

using LiteDB;
using Primify.Converters;
using Primify.Generator.Tests.Types;

public class LiteDbRegistrationTests
{
    [Test]
    public async Task UnsupportedWrapper_IsSkipped()
    {
        var mapper = new BsonMapper();

        var registered = LiteDbMapping.TryRegister<UnsupportedLiteDbWrapper, UnsupportedLiteDbValue>(mapper);

        await Assert.That(registered).IsFalse();
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
