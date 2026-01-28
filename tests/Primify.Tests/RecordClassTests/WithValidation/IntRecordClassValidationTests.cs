namespace Primify.Generator.Tests.RecordClassTests.WithValidation;

public class IntRecordClassValidationTests
{
    [Test]
    public async Task From_CreatesInstance_WhenValueIsValid()
    {
        var value = 42;
        var result = IntRecordClassWithValidation.From(value);
        
        await Assert.That(result.Value).IsEqualTo(value);
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntRecordClassWithValidation.From(-1); });
    }

    [Test]
    public async Task From_CreatesInstance_WhenValueIsZero()
    {
        var result = IntRecordClassWithValidation.From(0);
        
        await Assert.That(result.Value).IsEqualTo(0);
    }

    [Test]
    public async Task From_CreatesInstance_WhenValueIsOne()
    {
        var result = IntRecordClassWithValidation.From(1);
        
        await Assert.That(result.Value).IsEqualTo(1);
    }

    [Test]
    public async Task From_CreatesInstance_WhenValueIsOneHundred()
    {
        var result = IntRecordClassWithValidation.From(100);
        
        await Assert.That(result.Value).IsEqualTo(100);
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsNegativeOne()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntRecordClassWithValidation.From(-1); });
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsNegativeTen()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntRecordClassWithValidation.From(-10); });
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsNegativeHundred()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntRecordClassWithValidation.From(-100); });
    }
}
