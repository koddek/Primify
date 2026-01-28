namespace Primify.Generator.Tests.ClassTests.WithDefaultProperty;

public class IntClassDefaultPropertyTests
{
    [Test]
    public async Task Empty_ReturnsDefaultInstance()
    {
        var result = IntClassWithDefaultProperty.Empty;
        
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task From_CreatesInstance_WhenCalled()
    {
        var value = 42;
        var result = IntClassWithDefaultProperty.From(value);
        
        await Assert.That(result.Value).IsEqualTo(value);
    }

    [Test]
    public async Task Serialization_IgnoresStaticProperty_WhenSerializedWithSystemTextJson()
    {
        var result = IntClassWithDefaultProperty.Empty;
        var expectedValue = -1;

        await Assert.That(result.Value).IsEqualTo(expectedValue);

        var json = System.Text.Json.JsonSerializer.Serialize(result);

        var deserialized = System.Text.Json.JsonSerializer.Deserialize<IntClassWithDefaultProperty>(json);
        await Assert.That(deserialized!.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Serialization_IgnoresStaticProperty_WhenSerializedWithNewtonsoftJson()
    {
        var result = IntClassWithDefaultProperty.Empty;
        var expectedValue = -1;

        await Assert.That(result.Value).IsEqualTo(expectedValue);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);

        var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<IntClassWithDefaultProperty>(json);
        await Assert.That(deserialized!.Value).IsEqualTo(expectedValue);
    }
}
