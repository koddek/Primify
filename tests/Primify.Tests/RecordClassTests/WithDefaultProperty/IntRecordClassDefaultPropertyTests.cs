namespace Primify.Generator.Tests.RecordClassTests.WithDefaultProperty;

public class IntRecordClassDefaultPropertyTests
{
    [Test]
    public async Task Empty_ReturnsDefaultInstance()
    {
        var result = IntRecordClassWithDefaultProperty.Empty;
        
        await Assert.That(result.Value).IsEqualTo(-1);
    }

    [Test]
    public async Task From_CreatesInstance_WhenCalled()
    {
        var value = 42;
        var result = IntRecordClassWithDefaultProperty.From(value);
        
        await Assert.That(result.Value).IsEqualTo(value);
    }
}
