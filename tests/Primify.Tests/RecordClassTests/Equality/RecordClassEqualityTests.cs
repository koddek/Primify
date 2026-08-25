namespace Primify.Generator.Tests.RecordClassTests.Equality;

public class RecordClassEqualityTests
{
    [Test]
    public async Task Equals_ReturnsTrue_ForSameValue()
    {
        var a = RecordClassId.From(5);
        var b = RecordClassId.From(5);

        await Assert.That(a.Equals(b)).IsTrue();
        await Assert.That(a == b).IsTrue();
    }

    [Test]
    public async Task Equals_ReturnsFalse_ForDifferentValue()
    {
        var a = RecordClassId.From(1);
        var b = RecordClassId.From(2);

        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a != b).IsTrue();
    }

    [Test]
    public async Task GetHashCode_Matches_ForSameValue()
    {
        var a = RecordClassId.From(9);
        var b = RecordClassId.From(9);

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    [Test]
    public async Task HashSet_RejectsDuplicateValues()
    {
        var set = new HashSet<RecordClassId> { RecordClassId.From(4), RecordClassId.From(4) };

        await Assert.That(set.Count).IsEqualTo(1);
    }

    [Test]
    public async Task ToString_UsesRecordFormat()
    {
        var id = RecordClassId.From(7);

        await Assert.That(id.ToString()).IsEqualTo("RecordClassId { Value = 7 }");
    }

    [Test]
    public async Task SealedVariant_BehavesIdentically()
    {
        var a = SealedIntRecordClass.From(11);
        var b = SealedIntRecordClass.From(11);

        await Assert.That(a == b).IsTrue();
        await Assert.That(a.ToString()).IsEqualTo("SealedIntRecordClass { Value = 11 }");
    }
}
