using DentalClinic.Domain;

namespace DentalClinic.Tests.Unit.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("test+tag@example.org")]
    [InlineData("name123@test-domain.com")]
    public void Constructor_ValidEmail_ShouldCreateInstance(string validEmail)
    {
        // Act
        var email = new Email(validEmail);

        // Assert
        Assert.Equal(validEmail, email.Value);
    }

    [Fact]
    public void Constructor_NullEmail_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Email(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhiteSpaceEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(invalidEmail));
    }

    [Theory]
    [InlineData("test")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    [InlineData("test.example.com")]
    [InlineData("test@@example.com")]
    [InlineData("test@example")]
    [InlineData("test@.com")]
    public void Constructor_InvalidEmailFormat_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(invalidEmail));
    }

    [Fact]
    public void Constructor_EmailTooShort_ShouldThrowArgumentException()
    {
        // Arrange
        var shortEmail = "a@b.c"; // 5 characters, minimum is 6

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(shortEmail));
    }

    [Fact]
    public void Constructor_EmailTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longEmail = new string('a', 310) + "@example.com"; // > 320 characters

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(longEmail));
    }

    [Fact]
    public void Equality_SameEmails_ShouldBeEqual()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Act & Assert
        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
        Assert.False(email1 != email2);
    }

    [Fact]
    public void Equality_DifferentEmails_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Act & Assert
        Assert.NotEqual(email1, email2);
        Assert.False(email1 == email2);
        Assert.True(email1 != email2);
    }
} 