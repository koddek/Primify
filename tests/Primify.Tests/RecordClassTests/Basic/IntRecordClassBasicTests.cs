namespace Primify.Generator.Tests.RecordClassTests.Basic;

public class IntRecordClassBasicTests
{
    [Test]
    public async Task From_CreatesType_WhenCalled()
    {
        int expectedValue = 1001;
        var result = IntRecordClass.From(expectedValue);

        await Assert.That(result.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenSettingValue()
    {
        int expectedValue = 1001;
        IntRecordClass result = expectedValue;

        await Assert.That(result.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenDereferencing()
    {
        int expectedValue = 1001;
        var input = IntRecordClass.From(expectedValue);
        int result = input;

        await Assert.That(result).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ExplicitCast_CreatesType_WhenCalled()
    {
        int expectedValue = 1001;

        IntRecordClass result1 = (IntRecordClass)expectedValue;
        int result2 = (int)result1;

        await Assert.That(result1.Value).IsEqualTo(expectedValue);
        await Assert.That(result2).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Serialization_Works_WithSystemTextJson()
    {
        var expectedValue = 42;
        var result = IntRecordClass.From(expectedValue);

        var json = System.Text.Json.JsonSerializer.Serialize(result);

        var deserialized = System.Text.Json.JsonSerializer.Deserialize<IntRecordClass>(json);
        await Assert.That(deserialized!.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Serialization_Works_WithNewtonsoftJson()
    {
        var expectedValue = 42;
        var result = IntRecordClass.From(expectedValue);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);

        var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<IntRecordClass>(json);
        await Assert.That(deserialized!.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task Value_AccessesCorrectValue()
    {
        var value = 123;
        var wrapper = IntRecordClass.From(value);

        await Assert.That(wrapper.Value).IsEqualTo(value);
    }
}
