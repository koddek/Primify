namespace Primify.Generator.Tests.RecordStructTests.Basic;

public class IntRecordStructBasicTests
{
    [Test]
    public async Task From_CreatesType_WhenCalled()
    {
        int expectedValue = 1001;
        var result = IntRecordStruct.From(expectedValue);

        await Assert.That(result.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenSettingValue()
    {
        int expectedValue = 1001;
        IntRecordStruct result = expectedValue;

        await Assert.That(result.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Value_AccessesCorrectValue()
    {
        var value = 123;
        var wrapper = IntRecordStruct.From(value);

        await Assert.That(wrapper.Value).IsEqualTo(value);
    }

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenDereferencing()
    {
        int expectedValue = 1001;
        var input = IntRecordStruct.From(expectedValue);
        int result = input;

        await Assert.That(result).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ExplicitCast_CreatesType_WhenCalled()
    {
        int expectedValue = 1001;

        IntRecordStruct result1 = (IntRecordStruct)expectedValue;
        int result2 = (int)result1;

        await Assert.That(result1.Value).IsEqualTo(expectedValue);
        await Assert.That(result2).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Serialization_Works_WithSystemTextJson()
    {
        var expectedValue = 42;
        var result = IntRecordStruct.From(expectedValue);

        var json = System.Text.Json.JsonSerializer.Serialize(result);

        var deserialized = System.Text.Json.JsonSerializer.Deserialize<IntRecordStruct>(json);
        await Assert.That(deserialized.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Serialization_Works_WithNewtonsoftJson()
    {
        var expectedValue = 42;
        var result = IntRecordStruct.From(expectedValue);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);

        var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<IntRecordStruct>(json);
        await Assert.That(deserialized.Value).IsEqualTo(expectedValue);
    }
}
