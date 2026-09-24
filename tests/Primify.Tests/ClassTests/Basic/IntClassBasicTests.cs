namespace Primify.Generator.Tests.ClassTests.Basic;

using Primify.Generator.Tests.Common;

public class IntClassBasicTests
{
    [Test]
    public async Task From_CreatesType_WhenCalled() => await WrapperContract.From_CreatesValidInstance<IntClass>();

    [Test]
    public async Task Value_AccessesCorrectValue() => await WrapperContract.Value_ReturnsWrappedValue<IntClass>();

    [Test]
    public async Task ImplicitConversion_Succeeds_WhenDereferencing() => await WrapperContract.ImplicitConversion_ReturnsPrimitive<IntClass>(
        value => (int)value);

    [Test]
    public async Task ExplicitCast_CreatesType_WhenCalled() => await WrapperContract.ExplicitConversion_RoundTrips<IntClass>(
        value => (IntClass)value,
        value => (int)value);

    [Test]
    public async Task Serialization_Works_WithSystemTextJson() => await WrapperContract.SystemTextJson_RoundTrips<IntClass>();

    [Test]
    public async Task Serialization_Works_WithNewtonsoftJson() => await WrapperContract.NewtonsoftJson_RoundTrips<IntClass>();

    [Test]
    public async Task ToString_ReflectsUnderlyingValue() => await WrapperContract.ToString_ReturnsExpectedText<IntClass>("1001");
}
