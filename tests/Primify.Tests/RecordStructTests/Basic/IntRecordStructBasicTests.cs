namespace Primify.Generator.Tests.RecordStructTests.Basic;

using Primify.Generator.Tests.Common;

public class IntRecordStructBasicTests
{
    [Test]
    public async Task From_CreatesType_WhenCalled() => await WrapperContract.From_CreatesValidInstance<IntRecordStruct>();

    [Test]
    public async Task Value_AccessesCorrectValue() => await WrapperContract.Value_ReturnsWrappedValue<IntRecordStruct>();

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenDereferencing() => await WrapperContract.ImplicitConversion_ReturnsPrimitive<IntRecordStruct>(
        value => (int)value);

    [Test]
    public async Task ExplicitCast_CreatesType_WhenCalled() => await WrapperContract.ExplicitConversion_RoundTrips<IntRecordStruct>(
        value => (IntRecordStruct)value,
        value => (int)value);

    [Test]
    public async Task Serialization_Works_WithSystemTextJson() => await WrapperContract.SystemTextJson_RoundTrips<IntRecordStruct>();

    [Test]
    public async Task Serialization_Works_WithNewtonsoftJson() => await WrapperContract.NewtonsoftJson_RoundTrips<IntRecordStruct>();

    [Test]
    public async Task ToString_ReflectsUnderlyingValue() => await WrapperContract.ToString_ReturnsExpectedText<IntRecordStruct>("IntRecordStruct { Value = 1001 }");
}
