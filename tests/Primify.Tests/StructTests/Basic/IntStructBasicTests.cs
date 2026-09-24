namespace Primify.Generator.Tests.StructTests.Basic;

using Primify.Generator.Tests.Common;

public class IntStructBasicTests
{
    [Test]
    public async Task From_CreatesType_WhenCalled() => await WrapperContract.From_CreatesValidInstance<IntStruct>();

    [Test]
    public async Task Value_AccessesCorrectValue() => await WrapperContract.Value_ReturnsWrappedValue<IntStruct>();

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenDereferencing() => await WrapperContract.ImplicitConversion_ReturnsPrimitive<IntStruct>(
        value => (int)value);

    [Test]
    public async Task ExplicitCast_CreatesType_WhenCalled() => await WrapperContract.ExplicitConversion_RoundTrips<IntStruct>(
        value => (IntStruct)value,
        value => (int)value);

    [Test]
    public async Task Serialization_Works_WithSystemTextJson() => await WrapperContract.SystemTextJson_RoundTrips<IntStruct>();

    [Test]
    public async Task Serialization_Works_WithNewtonsoftJson() => await WrapperContract.NewtonsoftJson_RoundTrips<IntStruct>();

    [Test]
    public async Task ToString_ReflectsUnderlyingValue() => await WrapperContract.ToString_ReturnsExpectedText<IntStruct>("1001");
}
