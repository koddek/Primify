namespace Primify.Generator.Tests.RecordStructTests.NormalizeBeforeValidate;

public class StringRecordStructNormalizeBeforeValidateTests
{
    [Test]
    public async Task From_AllowsPaddedInput_ByNormalizingBeforeValidate()
    {
        var result = StringRecordStructWithNormalizeAndValidate.From(" abc ");

        await Assert.That(result.Value).IsEqualTo("abc");
    }

    [Test]
    public async Task From_ThrowsException_WhenNormalizedValueIsStillInvalid()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = StringRecordStructWithNormalizeAndValidate.From(" abcd "); });
    }
}
