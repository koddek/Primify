using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Primify.Generator.Tests.ClassTests.Basic;

public class IntClassBasicTests
{
    [Test]
    public async Task From_CreatesType_WhenCalled()
    {
        int expectedValue = 1001;
        var result = IntClass.From(expectedValue);

        await Assert.That(result.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenSettingValue()
    {
        int expectedValue = 1001;
        IntClass result = expectedValue;

        await Assert.That(result.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenDereferencing()
    {
        int expectedValue = 1001;
        var input = IntClass.From(expectedValue);
        int result = input;

        await Assert.That(result).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ExplicitCast_CreatesType_WhenCalled()
    {
        int expectedValue = 1001;

        IntClass result1 = (IntClass)expectedValue;
        int result2 = (int)result1;

        await Assert.That(result1.Value).IsEqualTo(expectedValue);
        await Assert.That(result2).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Serialization_Works_WithSystemTextJson()
    {
        var expectedValue = 42;
        var result = IntClass.From(expectedValue);

        var json = JsonSerializer.Serialize(result);

        var deserialized = JsonSerializer.Deserialize<IntClass>(json);
        await Assert.That(deserialized!.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Serialization_Works_WithNewtonsoftJson()
    {
        var expectedValue = 42;
        var result = IntClass.From(expectedValue);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);

        var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<IntClass>(json);
        await Assert.That(deserialized!.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Value_AccessesCorrectValue()
    {
        var value = 123;
        var wrapper = IntClass.From(value);

        await Assert.That(wrapper.Value).IsEqualTo(value);
    }
}
