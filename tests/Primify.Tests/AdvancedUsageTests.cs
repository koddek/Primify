using System;
using System.Threading.Tasks;
using Primify.Tests.Models;
using TUnit;
using TUnit.Assertions.AssertConditions.Throws;

namespace Primify.Tests;

public class AdvancedUsageTests
{
    [Test]
    [Arguments(0)]
    [Arguments(-5)]
    public async Task From_ThrowsArgumentException_WhenValueIsNonPositive(int value)
    {
        // Arrange
        var action = () => Quantity.From(value);

        // Act & Assert
        await Assert.That(action).ThrowsException();
        await Assert.That(action).ThrowsExactly<ArgumentException>();
        await Assert.That(action).Throws<ArgumentException>()
            .WithMessage("Quantity must be positive.");
    }

    [Test]
    [Arguments(1)]
    [Arguments(10)]
    public async Task From_ReturnsQuantity_WhenValueIsPositive(int value)
    {
        // Arrange
        var expected = Quantity.From(value);

        // Act
        var result = Quantity.From(value);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
        await Assert.That((int)result).IsEqualTo(value); // Implicit conversion
    }

    [Test]
    [Arguments("  ", "")]
    [Arguments("  Item Name  ", "Item Name")]
    [Arguments(null, "")]
    public async Task From_ReturnsNormalizedValue_WhenNormalizationApplied(string input, string expectedValue)
    {
        // Arrange
        var expected = ItemName.From(expectedValue);

        // Act
        var result = ItemName.From(input);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
        await Assert.That((string)result).IsEqualTo(expectedValue); // Implicit conversion
    }
}
