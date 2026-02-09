namespace Primify.Generator.Tests.RecordClassTests.AllFeatures;

public class IntRecordClassAllFeaturesTests
{
    [Test]
    public async Task Empty_ReturnsDefaultInstance()
    {
        var result = IntRecordClassWithAllFeatures.Empty;
        
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task From_CreatesInstance_WhenValueIsValid()
    {
        var value = 42;
        var result = IntRecordClassWithAllFeatures.From(value);
        
        await Assert.That(result.Value).IsEqualTo(value);
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntRecordClassWithAllFeatures.From(-1); });
    }

    [Test]
    public async Task From_ReturnsOne_WhenInputIsOne()
    {
        var result = IntRecordClassWithAllFeatures.From(1);
        await Assert.That(result.Value).IsEqualTo(1);
    }

    [Test]
    public async Task From_ReturnsTen_WhenInputIsTen()
    {
        var result = IntRecordClassWithAllFeatures.From(10);
        await Assert.That(result.Value).IsEqualTo(10);
    }

    [Test]
    public async Task From_ThrowsException_WhenInputIsZero()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntRecordClassWithAllFeatures.From(0); });
    }
}
