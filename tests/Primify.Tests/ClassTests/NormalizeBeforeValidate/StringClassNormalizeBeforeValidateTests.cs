namespace Primify.Generator.Tests.ClassTests.NormalizeBeforeValidate;

public class StringClassNormalizeBeforeValidateTests
{
    [Test]
    public async Task From_AllowsPaddedInput_ByNormalizingBeforeValidate()
    {
        var result = StringClassWithNormalizeAndValidate.From(" abc ");
        
        await Assert.That(result.Value).IsEqualTo("abc");
    }

    [Test]
    public async Task From_ThrowsException_WhenNormalizedValueIsStillInvalid()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = StringClassWithNormalizeAndValidate.From(" abcd "); });
    }
}
