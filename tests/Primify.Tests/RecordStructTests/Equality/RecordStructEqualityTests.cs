namespace Primify.Generator.Tests.RecordStructTests.Equality;

public class RecordStructEqualityTests
{
    [Test]
    public async Task Equals_ReturnsTrue_ForSameValue()
    {
        var a = RecordStructId.From(5);
        var b = RecordStructId.From(5);

        await Assert.That(a.Equals(b)).IsTrue();
        await Assert.That(a == b).IsTrue();
    }

    [Test]
    public async Task Equals_ReturnsFalse_ForDifferentValue()
    {
        var a = RecordStructId.From(1);
        var b = RecordStructId.From(2);

        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a != b).IsTrue();
    }

    [Test]
    public async Task GetHashCode_Matches_ForSameValue()
    {
        var a = RecordStructId.From(9);
        var b = RecordStructId.From(9);

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    [Test]
    public async Task HashSet_RejectsDuplicateValues()
    {
        var set = new HashSet<RecordStructId> { RecordStructId.From(4), RecordStructId.From(4) };

        await Assert.That(set.Count).IsEqualTo(1);
    }

    [Test]
    public async Task ReadonlyVariant_BehavesIdentically()
    {
        var a = ReadonlyIntRecordStruct.From(11);
        var b = ReadonlyIntRecordStruct.From(11);

        await Assert.That(a == b).IsTrue();
        await Assert.That(a.ToString()).IsEqualTo("ReadonlyIntRecordStruct { Value = 11 }");
    }
}
