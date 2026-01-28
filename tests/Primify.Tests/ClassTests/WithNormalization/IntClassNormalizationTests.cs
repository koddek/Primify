namespace Primify.Generator.Tests.ClassTests.WithNormalization;

public class IntClassNormalizationTests
{
    [Test]
    public async Task From_ReturnsOne_WhenInputIsOne()
    {
        var result = IntClassWithNormalization.From(1);
        await Assert.That(result.Value).IsEqualTo(1);
    }

    [Test]
    public async Task From_ReturnsTen_WhenInputIsTen()
    {
        var result = IntClassWithNormalization.From(10);
        await Assert.That(result.Value).IsEqualTo(10);
    }

    [Test]
    public async Task From_ReturnsNegativeOne_WhenInputIsZero()
    {
        var result = IntClassWithNormalization.From(0);
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task From_ReturnsNegativeOne_WhenInputIsNegative()
    {
        var result = IntClassWithNormalization.From(-1);
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task From_ReturnsNegativeOne_WhenInputIsNegativeHundred()
    {
        var result = IntClassWithNormalization.From(-100);
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task Value_ReflectsNormalizedValue_ForOne()
    {
        var wrapper = IntClassWithNormalization.From(1);
        await Assert.That(wrapper.Value).IsEqualTo(1);
    }

    [Test]
    public async Task Value_ReflectsNormalizedValue_ForZero()
    {
        var wrapper = IntClassWithNormalization.From(0);
        await Assert.That(wrapper.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task Value_ReflectsNormalizedValue_ForNegative()
    {
        var wrapper = IntClassWithNormalization.From(-1);
        await Assert.That(wrapper.Value).IsEqualTo(-1);
    }
}
