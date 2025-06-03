using DentalClinic.Domain.Aggregates.ServiceAggregate;

namespace DentalClinic.Tests.Unit.Domain.ValueObjects;

public class PriceTests
{
    [Theory]
    [InlineData(1.0)]
    [InlineData(100.50)]
    [InlineData(999.99)]
    [InlineData(0.01)]
    public void Constructor_ValidPrice_ShouldCreateInstance(decimal validPrice)
    {
        // Act
        var price = new Price(validPrice);

        // Assert
        Assert.Equal(validPrice, price.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Constructor_NegativeOrZeroPrice_ShouldThrowArgumentOutOfRangeException(decimal invalidPrice)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Price(invalidPrice));
    }

    [Fact]
    public void Constructor_MaxDecimalValue_ShouldCreateInstance()
    {
        // Act
        var price = new Price(decimal.MaxValue);

        // Assert
        Assert.Equal(decimal.MaxValue, price.Value);
    }

    [Fact]
    public void Constructor_MinPositiveDecimalValue_ShouldCreateInstance()
    {
        // Arrange
        var minPositiveValue = 0.0000000000000000001m;

        // Act
        var price = new Price(minPositiveValue);

        // Assert
        Assert.Equal(minPositiveValue, price.Value);
    }

    [Fact]
    public void Equality_SamePrices_ShouldBeEqual()
    {
        // Arrange
        var price1 = new Price(100.50m);
        var price2 = new Price(100.50m);

        // Act & Assert
        Assert.Equal(price1, price2);
        Assert.True(price1 == price2);
        Assert.False(price1 != price2);
    }

    [Fact]
    public void Equality_DifferentPrices_ShouldNotBeEqual()
    {
        // Arrange
        var price1 = new Price(100.50m);
        var price2 = new Price(200.75m);

        // Act & Assert
        Assert.NotEqual(price1, price2);
        Assert.False(price1 == price2);
        Assert.True(price1 != price2);
    }

    [Fact]
    public void GetHashCode_SamePrices_ShouldHaveSameHashCode()
    {
        // Arrange
        var price1 = new Price(100.50m);
        var price2 = new Price(100.50m);

        // Act & Assert
        Assert.Equal(price1.GetHashCode(), price2.GetHashCode());
    }
} 