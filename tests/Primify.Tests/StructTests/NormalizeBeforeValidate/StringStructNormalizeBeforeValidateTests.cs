namespace Primify.Generator.Tests.StructTests.NormalizeBeforeValidate;

public class StringStructNormalizeBeforeValidateTests
{
    [Test]
    public async Task From_AllowsPaddedInput_ByNormalizingBeforeValidate()
    {
        var result = StringStructWithNormalizeAndValidate.From(" abc ");

        await Assert.That(result.Value).IsEqualTo("abc");
    }

    [Test]
    public async Task From_ThrowsException_WhenNormalizedValueIsStillInvalid()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = StringStructWithNormalizeAndValidate.From(" abcd "); });
    }
}
