namespace Primify.Generator.Tests.ClassTests.WithValidation;

public class IntClassValidationTests
{
    [Test]
    public async Task From_CreatesInstance_WhenValueIsValid()
    {
        var value = 42;
        var result = IntClassWithValidation.From(value);
        
        await Assert.That(result.Value).IsEqualTo(value);
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsInvalid()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntClassWithValidation.From(-1); });
    }

    [Test]
    public async Task From_CreatesInstance_WhenValueIsZero()
    {
        var result = IntClassWithValidation.From(0);
        
        await Assert.That(result.Value).IsEqualTo(0);
    }

    [Test]
    public async Task From_CreatesInstance_WhenValueIsOne()
    {
        var result = IntClassWithValidation.From(1);
        
        await Assert.That(result.Value).IsEqualTo(1);
    }

    [Test]
    public async Task From_CreatesInstance_WhenValueIsOneHundred()
    {
        var result = IntClassWithValidation.From(100);
        
        await Assert.That(result.Value).IsEqualTo(100);
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsNegativeOne()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntClassWithValidation.From(-1); });
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsNegativeTen()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntClassWithValidation.From(-10); });
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsNegativeHundred()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntClassWithValidation.From(-100); });
    }

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenValueIsValid()
    {
        int value = 42;
        IntClassWithValidation result = value;
        
        await Assert.That(result.Value).IsEqualTo(value);
    }
}
