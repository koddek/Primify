namespace Primify.Generator.Tests.StructTests.Equality;

public class StructEqualityTests
{
    [Test]
    public async Task Equals_ReturnsTrue_ForSameValue()
    {
        var a = StructId.From(5);
        var b = StructId.From(5);

        await Assert.That(a.Equals(b)).IsTrue();
        await Assert.That(a == b).IsTrue();
    }

    [Test]
    public async Task Equals_ReturnsFalse_ForDifferentValue()
    {
        var a = StructId.From(1);
        var b = StructId.From(2);

        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a != b).IsTrue();
    }

    [Test]
    public async Task GetHashCode_Matches_ForSameValue()
    {
        var a = StructId.From(9);
        var b = StructId.From(9);

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    [Test]
    public async Task BoxedEquals_WorksWithObjectOverload()
    {
        var a = StructId.From(3);
        object boxed = StructId.From(3);

        await Assert.That(a.Equals(boxed)).IsTrue();
    }

    [Test]
    public async Task HashSet_RejectsDuplicateValues()
    {
        var set = new HashSet<StructId> { StructId.From(4), StructId.From(4) };

        await Assert.That(set.Count).IsEqualTo(1);
    }

    [Test]
    public async Task ReadonlyVariant_BehavesIdentically()
    {
        var a = ReadonlyIntStruct.From(11);
        var b = ReadonlyIntStruct.From(11);

        await Assert.That(a == b).IsTrue();
        await Assert.That(a.ToString()).IsEqualTo("11");
    }
}
