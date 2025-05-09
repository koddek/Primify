namespace Primify.Tests;

[Attributes.Primify<string>]
public partial record class ClassItem;

[Attributes.Primify<string>]
public readonly partial record struct StructItem;

public class ClassVsStructTests
{
    [Test]
    public async Task From_Succeds_WithClass()
    {
        // Arrange
        var value = "Shoe";

        // Act
        var result = ClassItem.From(value);

        // Assert
        await Assert.That(result.Value).IsEquivalentTo(value);
    }

    [Test]
    public async Task From_Succeds_WithStruct()
    {
        // Arrange
        var value = "Shoe";

        // Act
        var result = StructItem.From(value);

        // Assert
        await Assert.That(result.Value).IsEquivalentTo(value);
    }
}
