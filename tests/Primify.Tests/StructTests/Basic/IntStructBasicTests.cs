using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Primify.Generator.Tests.StructTests.Basic;

public class IntStructBasicTests
{
    [Test]
    public async Task From_CreatesType_WhenCalled()
    {
        int expectedValue = 1001;
        var result = IntStruct.From(expectedValue);

        await Assert.That(result.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenSettingValue()
    {
        int expectedValue = 1001;
        IntStruct result = expectedValue;

        await Assert.That(result.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenDereferencing()
    {
        int expectedValue = 1001;
        var input = IntStruct.From(expectedValue);
        int result = input;

        await Assert.That(result).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Value_AccessesCorrectValue()
    {
        var value = 123;
        var wrapper = IntStruct.From(value);

        await Assert.That(wrapper.Value).IsEqualTo(value);
    }

    [Test]
    public async Task Serialization_Works_WithSystemTextJson()
    {
        var expectedValue = 42;
        var result = IntStruct.From(expectedValue);

        var json = JsonSerializer.Serialize(result);

        var deserialized = JsonSerializer.Deserialize<IntStruct>(json);
        await Assert.That(deserialized.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ExplicitCast_CreatesType_WhenCalled()
    {
        int expectedValue = 1001;

        IntStruct result1 = (IntStruct)expectedValue;
        int result2 = (int)result1;

        await Assert.That(result1.Value).IsEqualTo(expectedValue);
        await Assert.That(result2).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Serialization_Works_WithNewtonsoftJson()
    {
        var expectedValue = 42;
        var result = IntStruct.From(expectedValue);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);

        var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<IntStruct>(json);
        await Assert.That(deserialized.Value).IsEqualTo(expectedValue);
    }
}
