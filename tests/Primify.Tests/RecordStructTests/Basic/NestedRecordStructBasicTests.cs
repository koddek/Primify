namespace Primify.Generator.Tests.RecordStructTests.Basic;

public class NestedRecordStructBasicTests
{
    [Test]
    public async Task From_CreatesNestedMaleId_WhenCalled()
    {
        var expectedValue = "male-1";

        var result = IUsers.Male.MaleId.From(expectedValue);

        await Assert.That(result.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task From_CreatesNestedFemaleId_WhenCalled()
    {
        var expectedValue = "female-1";

        var result = IUsers.Female.FemaleId.From(expectedValue);

        await Assert.That(result.Value).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task ImplicitConversions_Work_ForNestedIds()
    {
        IUsers.Male.MaleId maleId = "male-2";
        IUsers.Female.FemaleId femaleId = "female-2";

        string maleValue = maleId;
        string femaleValue = femaleId;

        await Assert.That(maleValue).IsEqualTo("male-2");
        await Assert.That(femaleValue).IsEqualTo("female-2");
    }

    [Test]
    public async Task NestedIds_CanBeAssigned_ToContainingTypes()
    {
        var male = new IUsers.Male { Id = "male-3" };
        var female = new IUsers.Female { Id = "female-3" };

        await Assert.That(male.Id.Value).IsEqualTo("male-3");
        await Assert.That(female.Id.Value).IsEqualTo("female-3");
    }
}
