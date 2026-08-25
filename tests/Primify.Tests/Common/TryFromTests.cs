namespace Primify.Generator.Tests.Common;

public class TryFromTests
{
    [Test]
    public async Task TryFrom_ReturnsTrue_AndValue_WhenValid()
    {
        var ok = IntStructWithValidation.TryFrom(5, out var result);

        await Assert.That(ok).IsTrue();
        await Assert.That(result.Value).IsEqualTo(5);
    }

    [Test]
    public async Task TryFrom_ReturnsFalse_WhenValidationThrows()
    {
        var ok = IntStructWithValidation.TryFrom(-1, out var result);

        await Assert.That(ok).IsFalse();
        await Assert.That(result).IsEqualTo(default(IntStructWithValidation));
    }

    [Test]
    public async Task TryFrom_AppliesNormalization_BeforeValidation()
    {
        var ok = StringClassWithNormalizeAndValidate.TryFrom(" ab ", out var result);

        await Assert.That(ok).IsTrue();
        await Assert.That(result!.Value).IsEqualTo("ab");
    }

    [Test]
    public async Task TryFrom_ReturnsFalse_WhenNormalizedValueStillInvalid()
    {
        var ok = StringClassWithNormalizeAndValidate.TryFrom(" abcd ", out _);

        await Assert.That(ok).IsFalse();
    }

    [Test]
    public async Task TryFrom_ReturnsFalse_ForNullString()
    {
        var ok = StringStruct.TryFrom(null!, out var result);

        await Assert.That(ok).IsFalse();
        await Assert.That(result).IsEqualTo(default(StringStruct));
    }

    [Test]
    public async Task TryFrom_Works_OnRecordClass()
    {
        var ok = IntRecordClassWithValidation.TryFrom(10, out var result);

        await Assert.That(ok).IsTrue();
        await Assert.That(result!.Value).IsEqualTo(10);
    }
}
