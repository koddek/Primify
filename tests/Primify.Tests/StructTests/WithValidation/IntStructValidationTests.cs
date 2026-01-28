namespace Primify.Generator.Tests.StructTests.WithValidation;

public class IntStructValidationTests
{
    [Test]
    public async Task From_CreatesInstance_WhenValueIsValid()
    {
        var value = 42;
        var result = IntStructWithValidation.From(value);
        
        await Assert.That(result.Value).IsEqualTo(value);
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsInvalid()
    {
        var value = -1;
        
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntStructWithValidation.From(value); });
    }

    [Test]
    public async Task From_CreatesInstance_WhenValueIsZero()
    {
        var result = IntStructWithValidation.From(0);
        
        await Assert.That(result.Value).IsEqualTo(0);
    }

    [Test]
    public async Task From_ThrowsException_WhenValueIsNegative()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => { _ = IntStructWithValidation.From(-10); });
    }
}
