namespace Primify.Generator.Tests.StructTests.WithNormalization;

public class IntStructNormalizationTests
{
    [Test]
    public async Task From_ReturnsOne_WhenInputIsOne()
    {
        var result = IntStructWithNormalization.From(1);
        await Assert.That(result.Value).IsEqualTo(1);
    }

    [Test]
    public async Task From_ReturnsTen_WhenInputIsTen()
    {
        var result = IntStructWithNormalization.From(10);
        await Assert.That(result.Value).IsEqualTo(10);
    }

    [Test]
    public async Task From_ReturnsNegativeOne_WhenInputIsZero()
    {
        var result = IntStructWithNormalization.From(0);
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task From_ReturnsNegativeOne_WhenInputIsNegative()
    {
        var result = IntStructWithNormalization.From(-1);
        await Assert.That(result.Value).IsEqualTo(-1);
    }
}
