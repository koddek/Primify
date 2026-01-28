namespace Primify.Generator.Tests.RecordStructTests.WithValidation;

public class IntRecordStructValidationTests
{
    [Test]
    public async Task From_CreatesInstance_WhenValueIsValid()
    {
        var value = 42;
        var result = IntRecordStructWithValidation.From(value);
        
        await Assert.That(result.Value).IsEqualTo(value);
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntRecordStructWithValidation.From(-1); });
    }

    [Test]
    public async Task From_CreatesInstance_WhenValueIsZero()
    {
        var result = IntRecordStructWithValidation.From(0);
        
        await Assert.That(result.Value).IsEqualTo(0);
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsNegative()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntRecordStructWithValidation.From(-10); });
    }
}
