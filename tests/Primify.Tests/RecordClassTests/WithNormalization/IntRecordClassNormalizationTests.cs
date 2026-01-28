namespace Primify.Generator.Tests.RecordClassTests.WithNormalization;

public class IntRecordClassNormalizationTests
{
    [Test]
    public async Task From_ReturnsOne_WhenInputIsOne()
    {
        var result = IntRecordClassWithNormalization.From(1);
        await Assert.That(result.Value).IsEqualTo(1);
    }

    [Test]
    public async Task From_ReturnsTen_WhenInputIsTen()
    {
        var result = IntRecordClassWithNormalization.From(10);
        await Assert.That(result.Value).IsEqualTo(10);
    }

    [Test]
    public async Task From_ReturnsNegativeOne_WhenInputIsZero()
    {
        var result = IntRecordClassWithNormalization.From(0);
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task From_ReturnsNegativeOne_WhenInputIsNegative()
    {
        var result = IntRecordClassWithNormalization.From(-1);
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task From_ReturnsNegativeOne_WhenInputIsNegativeHundred()
    {
        var result = IntRecordClassWithNormalization.From(-100);
        await Assert.That(result.Value).IsEqualTo(-1);
    }
}
