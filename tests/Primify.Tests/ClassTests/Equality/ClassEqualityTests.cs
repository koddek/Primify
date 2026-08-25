namespace Primify.Generator.Tests.ClassTests.Equality;

public class ClassEqualityTests
{
    [Test]
    public async Task Equals_ReturnsTrue_ForSameValue()
    {
        var a = ClassId.From(5);
        var b = ClassId.From(5);

        await Assert.That(a.Equals(b)).IsTrue();
        await Assert.That(a == b).IsTrue();
    }

    [Test]
    public async Task Equals_ReturnsFalse_ForDifferentValue()
    {
        var a = ClassId.From(1);
        var b = ClassId.From(2);

        await Assert.That(a.Equals(b)).IsFalse();
        await Assert.That(a != b).IsTrue();
    }

    [Test]
    public async Task Equals_HandlesNullSafely()
    {
        ClassId? none = null;

        await Assert.That(none == null).IsTrue();
        await Assert.That(ClassId.From(1) != none).IsTrue();
    }

    [Test]
    public async Task GetHashCode_Matches_ForSameValue()
    {
        var a = ClassId.From(9);
        var b = ClassId.From(9);

        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    [Test]
    public async Task HashSet_RejectsDuplicateValues()
    {
        var set = new HashSet<ClassId> { ClassId.From(4), ClassId.From(4) };

        await Assert.That(set.Count).IsEqualTo(1);
    }

    [Test]
    public async Task SealedVariant_BehavesIdentically()
    {
        var a = SealedIntClass.From(11);
        var b = SealedIntClass.From(11);

        await Assert.That(a == b).IsTrue();
        await Assert.That(a.ToString()).IsEqualTo("11");
    }
}
