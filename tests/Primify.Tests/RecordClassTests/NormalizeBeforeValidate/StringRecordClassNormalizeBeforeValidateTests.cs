namespace Primify.Generator.Tests.RecordClassTests.NormalizeBeforeValidate;

public class StringRecordClassNormalizeBeforeValidateTests
{
    [Test]
    public async Task From_AllowsPaddedInput_ByNormalizingBeforeValidate()
    {
        var result = StringRecordClassWithNormalizeAndValidate.From(" abc ");

        await Assert.That(result.Value).IsEqualTo("abc");
    }

    [Test]
    public async Task From_ThrowsException_WhenNormalizedValueIsStillInvalid()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = StringRecordClassWithNormalizeAndValidate.From(" abcd "); });
    }
}
