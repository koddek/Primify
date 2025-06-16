namespace Primify.Generator.Tests.ClassEqualityTests;

public class SealedClassEqualityTests
{
    //================================================================================
    // Construction and Casting Tests
    //================================================================================

    [Fact]
    public void From_CreatesInstanceWithCorrectValue()
    {
        // Arrange
        var value = 123;

        // Act
        var objectId = SealedClassId.From(value);

        // Assert
        Assert.NotNull(objectId);
        Assert.Equal(value, objectId.Value);
    }

    [Fact]
    public void ExplicitCast_FromUnderlyingType_CreatesInstance()
    {
        // Arrange
        var value = 456;

        // Act
        var objectId = (SealedClassId)value;

        // Assert
        Assert.NotNull(objectId);
        Assert.Equal(value, objectId.Value);
    }

    [Fact]
    public void ExplicitCast_ToUnderlyingType_ReturnsValue()
    {
        // Arrange
        var value = 789;
        var objectId = SealedClassId.From(value);

        // Act
        int result = (int)objectId;

        // Assert
        Assert.Equal(value, result);
    }

    //================================================================================
    // ToString() Test
    //================================================================================

    [Fact]
    public void ToString_ReturnsRecordLikeFormat()
    {
        // Arrange
        var objectId = SealedClassId.From(123);
        var expectedString = $"{nameof(SealedClassId)} {{ Value = 123 }}";

        // Act
        var result = objectId.ToString();

        // Assert
        Assert.Equal(expectedString, result);
    }

    //================================================================================
    // Equality and Comparison Tests
    //================================================================================

    [Fact]
    public void Equality_ForSameValue_ShouldBeTrue()
    {
        // Arrange
        var objectId1 = SealedClassId.From(100);
        var objectId2 = SealedClassId.From(100); // Different instance, same value

        // Assert
        Assert.True(objectId1.Equals(objectId2)); // IEquatable<T>.Equals
        Assert.True(objectId1.Equals((object)objectId2)); // object.Equals
        Assert.True(objectId1 == objectId2); // Operator ==
        Assert.False(objectId1 != objectId2); // Operator !=
    }

    [Fact]
    public void Equality_ForDifferentValues_ShouldBeFalse()
    {
        // Arrange
        var objectId1 = SealedClassId.From(100);
        var objectId2 = SealedClassId.From(200);

        // Assert
        Assert.False(objectId1.Equals(objectId2));
        Assert.False(objectId1.Equals((object)objectId2));
        Assert.False(objectId1 == objectId2);
        Assert.True(objectId1 != objectId2);
    }

    [Fact]
    public void ReferenceEquality_ForSameInstance_ShouldBeTrue()
    {
        // Arrange
        var objectId1 = SealedClassId.From(100);
        var objectId2 = objectId1; // Same instance

        // Assert
        Assert.True(objectId1.Equals(objectId2));
        Assert.True(objectId1 == objectId2);
        Assert.Same(objectId1, objectId2); // Verifies they are the exact same object
    }

    [Fact]
    public void Equality_WithNull_ShouldBeFalse()
    {
        // Arrange
        var objectId = SealedClassId.From(100);

        // Assert
        Assert.False(objectId.Equals(null));
        Assert.False(objectId == null);
        Assert.False(null == objectId);
        Assert.True(objectId != null);
        Assert.True(null != objectId);
    }

    [Fact]
    public void Equality_ForTwoNulls_ShouldBeTrue()
    {
        // Arrange
        SealedClassId? null1 = null;
        SealedClassId? null2 = null;

        // Assert
        Assert.True(null1 == null2);
        Assert.False(null1 != null2);
    }

    [Fact]
    public void Equality_WithDifferentType_ShouldBeFalse()
    {
        // Arrange
        var objectId = SealedClassId.From(100);
        var otherObject = new { Value = 100 };

        // Assert
        Assert.False(objectId.Equals(otherObject));
    }

    //================================================================================
    // GetHashCode() Tests
    //================================================================================

    [Fact]
    public void GetHashCode_ForEqualObjects_ShouldBeEqual()
    {
        // Arrange
        var objectId1 = SealedClassId.From(100);
        var objectId2 = SealedClassId.From(100);

        // Act & Assert
        Assert.Equal(objectId1.GetHashCode(), objectId2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ForDifferentObjects_ShouldUsuallyBeDifferent()
    {
        // Arrange
        var objectId1 = SealedClassId.From(100);
        var objectId2 = SealedClassId.From(200);

        // Act & Assert
        // Note: While not a strict guarantee, for primitive integer types,
        // the hash codes will be different.
        Assert.NotEqual(objectId1.GetHashCode(), objectId2.GetHashCode());
    }

    //================================================================================
    // Collection Behavior Tests
    //================================================================================

    [Fact]
    public void HashSet_ShouldNotAllowDuplicateValues()
    {
        // Arrange
        var set = new HashSet<SealedClassId>();
        var objectId1 = SealedClassId.From(100);
        var objectId2 = SealedClassId.From(100); // Equal, but different instance

        // Act
        var added1 = set.Add(objectId1);
        var added2 = set.Add(objectId2); // This should fail

        // Assert
        Assert.True(added1);
        Assert.False(added2);
        Assert.Single(set); // Confirms only one item is in the set
    }

    [Fact]
    public void Dictionary_ShouldRetrieveValueWithEqualKey()
    {
        // Arrange
        var dictionary = new Dictionary<SealedClassId, string>();
        var key1 = SealedClassId.From(50);
        var key2 = SealedClassId.From(50); // Equal, but different instance
        var value = "Test Value";

        // Act
        dictionary.Add(key1, value);

        // Assert
        Assert.True(dictionary.ContainsKey(key2));
        Assert.Equal(value, dictionary[key2]);
    }
}
