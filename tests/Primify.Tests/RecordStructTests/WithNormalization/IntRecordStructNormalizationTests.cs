namespace Primify.Generator.Tests.RecordStructTests.WithNormalization;

public class IntRecordStructNormalizationTests
{
    [Test]
    public async Task From_ReturnsOne_WhenInputIsOne()
    {
        var result = IntRecordStructWithNormalization.From(1);
        await Assert.That(result.Value).IsEqualTo(1);
    }

    [Test]
    public async Task From_ReturnsTen_WhenInputIsTen()
    {
        var result = IntRecordStructWithNormalization.From(10);
        await Assert.That(result.Value).IsEqualTo(10);
    }

    [Test]
    public async Task From_ReturnsNegativeOne_WhenInputIsZero()
    {
        var result = IntRecordStructWithNormalization.From(0);
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task From_ReturnsNegativeOne_WhenInputIsNegative()
    {
        var result = IntRecordStructWithNormalization.From(-1);
        await Assert.That(result.Value).IsEqualTo(-1);
    }
}
