namespace Primify.Generator.Tests.RecordStructTests.AllFeatures;

public class IntRecordStructAllFeaturesTests
{
    [Test]
    public async Task Empty_ReturnsDefaultInstance()
    {
        var result = IntRecordStructWithAllFeatures.Empty;
        
        await Assert.That(result.Value).IsEqualTo(0);
    }

    [Test]
    public async Task From_CreatesInstance_WhenValueIsValid()
    {
        var value = 42;
        var result = IntRecordStructWithAllFeatures.From(value);
        
        await Assert.That(result.Value).IsEqualTo(value);
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntRecordStructWithAllFeatures.From(101); });
    }

    [Test]
    public async Task From_ReturnsOne_WhenInputIsOne()
    {
        var result = IntRecordStructWithAllFeatures.From(1);
        await Assert.That(result.Value).IsEqualTo(1);
    }

    [Test]
    public async Task From_ReturnsTen_WhenInputIsTen()
    {
        var result = IntRecordStructWithAllFeatures.From(10);
        await Assert.That(result.Value).IsEqualTo(10);
    }

    [Test]
    public async Task From_ReturnsZero_WhenInputIsZero()
    {
        var result = IntRecordStructWithAllFeatures.From(0);
        
        await Assert.That(result.Value).IsEqualTo(0);
    }
}
