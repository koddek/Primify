namespace Primify.Generator.Tests.Common;

public class NestedTypeTests
{
    [Test]
    public async Task NestedRecordStruct_InInterface_CreatesAndConverts()
    {
        var maleId = (IUsers.Male.MaleId)"male-1";
        var femaleId = IUsers.Female.FemaleId.From("female-1");

        string maleValue = maleId;
        string femaleValue = femaleId;

        await Assert.That(maleValue).IsEqualTo("male-1");
        await Assert.That(femaleValue).IsEqualTo("female-1");
    }

    [Test]
    public async Task NestedRecordStruct_AssignsToContainingTypeProperty()
    {
        var male = new IUsers.Male { Id = (IUsers.Male.MaleId)"male-2" };

        await Assert.That(male.Id.Value).IsEqualTo("male-2");
    }

    [Test]
    public async Task NestedStruct_InStruct_CreatesAndDereferences()
    {
        var id = (NestedStructContainer.InnerStructId)42;

        int value = id;

        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task NestedRecordClass_InRecordClass_CreatesAndDereferences()
    {
        var id = (NestedRecordClassContainer.InnerRecordClassId)"inner";

        string value = id;

        await Assert.That(value).IsEqualTo("inner");
    }
}
