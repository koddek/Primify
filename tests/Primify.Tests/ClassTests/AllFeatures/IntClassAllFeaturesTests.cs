namespace Primify.Generator.Tests.ClassTests.AllFeatures;

public class IntClassAllFeaturesTests
{
    [Test]
    public async Task Empty_ReturnsDefaultInstance()
    {
        var result = IntClassWithAllFeatures.Empty;
        
        await Assert.That(result.Value).IsEqualTo(0);
    }

    [Test]
    public async Task From_CreatesInstance_WhenValueIsValid()
    {
        var value = 42;
        var result = IntClassWithAllFeatures.From(value);
        
        await Assert.That(result.Value).IsEqualTo(value);
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntClassWithAllFeatures.From(101); });
    }

    [Test]
    public async Task From_ReturnsOne_WhenInputIsOne()
    {
        var result = IntClassWithAllFeatures.From(1);
        await Assert.That(result.Value).IsEqualTo(1);
    }

    [Test]
    public async Task From_ReturnsTen_WhenInputIsTen()
    {
        var result = IntClassWithAllFeatures.From(10);
        await Assert.That(result.Value).IsEqualTo(10);
    }

    [Test]
    public async Task From_ReturnsZero_WhenInputIsZero()
    {
        var result = IntClassWithAllFeatures.From(0);
        
        await Assert.That(result.Value).IsEqualTo(0);
    }

    [Test]
    public async Task Serialization_Works_WithSystemTextJson()
    {
        var result = IntClassWithAllFeatures.From(42);
        var json = System.Text.Json.JsonSerializer.Serialize(result);

        var deserialized = System.Text.Json.JsonSerializer.Deserialize<IntClassWithAllFeatures>(json);
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Value).IsEqualTo(42);
    }

    [Test]
    public async Task Serialization_Works_WithNewtonsoftJson()
    {
        var result = IntClassWithAllFeatures.From(42);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);

        var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<IntClassWithAllFeatures>(json);
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Value).IsEqualTo(42);
    }
}
