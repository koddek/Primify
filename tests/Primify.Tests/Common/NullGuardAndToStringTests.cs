namespace Primify.Generator.Tests.Common;

public class NullGuardAndToStringTests
{
    [Test]
    public async Task From_ThrowsArgumentNullException_WhenWrappedStringIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => { _ = StringStruct.From(null!); });
    }

    [Test]
    public async Task ToString_ReturnsRawValue_ForPlainWrapperType()
    {
        var wrapper = StringStruct.From("hello");

        await Assert.That(wrapper.ToString()).IsEqualTo("hello");
    }

    [Test]
    public async Task ToString_ReturnsEmpty_WhenDefaultStructHasNullValue()
    {
        StringStruct wrapper = default;

        await Assert.That(wrapper.ToString()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ToString_PrintsRecordFormat_ForRecordWrapperType()
    {
        var wrapper = StringRecordClass.From("hello");

        await Assert.That(wrapper!.ToString()).IsEqualTo("StringRecordClass { Value = hello }");
    }
}
