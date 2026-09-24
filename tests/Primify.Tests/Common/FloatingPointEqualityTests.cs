namespace Primify.Generator.Tests.Common;

using Primify.Generator.Tests.Types;

public class FloatingPointEqualityTests
{
    [Test]
    public async Task FloatNaN_IsReflexive()
    {
        var value = FloatValueWrapper.From(float.NaN);
        var same = FloatValueWrapper.From(float.NaN);

        await Assert.That(value.Equals(value)).IsTrue();
        await Assert.That(value.Equals(same)).IsTrue();
        await Assert.That(value == same).IsTrue();
    }

    [Test]
    public async Task DoubleNaN_IsReflexive()
    {
        var value = DoubleValueWrapper.From(double.NaN);
        var same = DoubleValueWrapper.From(double.NaN);

        await Assert.That(value.Equals(value)).IsTrue();
        await Assert.That(value.Equals(same)).IsTrue();
        await Assert.That(value == same).IsTrue();
    }

    [Test]
    public async Task HalfNaN_IsReflexive()
    {
        var value = HalfValueWrapper.From(Half.NaN);
        var same = HalfValueWrapper.From(Half.NaN);

        await Assert.That(value.Equals(value)).IsTrue();
        await Assert.That(value.Equals(same)).IsTrue();
        await Assert.That(value == same).IsTrue();
    }
}
