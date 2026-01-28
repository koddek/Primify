namespace Primify.Generator.Tests.StructTests.WithDefaultProperty;

public class IntStructDefaultPropertyTests
{
    [Test]
    public async Task Empty_ReturnsDefaultInstance()
    {
        var result = IntStructWithDefaultProperty.Empty;
        
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task From_CreatesInstance_WhenCalled()
    {
        var value = 42;
        var result = IntStructWithDefaultProperty.From(value);
        
        await Assert.That(result.Value).IsEqualTo(value);
    }
}
