using DentalClinic.Domain.Aggregates.ServiceAggregate;

namespace DentalClinic.Tests.Unit.Domain.Aggregates;

public class ServiceTests
{
    [Fact]
    public void Constructor_ShouldGenerateNewId()
    {
        // Arrange & Act
        var service = CreateValidService();

        // Assert
        Assert.NotEqual(default, service.Id);
        Assert.NotEqual(Guid.Empty, service.Id.Value);
    }

    [Fact]
    public void Name_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var service = CreateValidService();
        var newName = "Root Canal Treatment";

        // Act
        service.Name = newName;

        // Assert
        Assert.Equal(newName, service.Name);
    }

    [Fact]
    public void Name_Null_ShouldThrowArgumentNullException()
    {
        // Arrange
        var service = CreateValidService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.Name = null!);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Name_WhiteSpace_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var service = CreateValidService();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.Name = invalidName);
    }

    [Fact]
    public void Price_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var service = CreateValidService();
        var newPrice = new Price(150.75m);

        // Act
        service.Price = newPrice;

        // Assert
        Assert.Equal(newPrice, service.Price);
    }

    [Fact]
    public void Price_Null_ShouldThrowArgumentNullException()
    {
        // Arrange
        var service = CreateValidService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.Price = null!);
    }

    [Fact]
    public void TwoServices_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var service1 = CreateValidService();
        var service2 = CreateValidService();

        // Act & Assert
        Assert.NotEqual(service1.Id, service2.Id);
    }

    [Fact]
    public void Service_WithSameNameAndPrice_ShouldHaveDifferentIds()
    {
        // Arrange
        var name = "Dental Cleaning";
        var price = new Price(100.00m);

        // Act
        var service1 = new Service { Name = name, Price = price };
        var service2 = new Service { Name = name, Price = price };

        // Assert
        Assert.NotEqual(service1.Id, service2.Id);
        Assert.Equal(service1.Name, service2.Name);
        Assert.Equal(service1.Price, service2.Price);
    }

    private static Service CreateValidService()
    {
        return new Service
        {
            Name = "Dental Cleaning",
            Price = new Price(100.00m)
        };
    }
} 