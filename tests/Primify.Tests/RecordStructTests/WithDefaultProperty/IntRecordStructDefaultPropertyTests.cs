namespace Primify.Generator.Tests.RecordStructTests.WithDefaultProperty;

public class IntRecordStructDefaultPropertyTests
{
    [Test]
    public async Task Empty_ReturnsDefaultInstance()
    {
        var result = IntRecordStructWithDefaultProperty.Empty;
        
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task From_CreatesInstance_WhenCalled()
    {
        var value = 42;
        var result = IntRecordStructWithDefaultProperty.From(value);
        
        await Assert.That(result.Value).IsEqualTo(value);
    }
}
