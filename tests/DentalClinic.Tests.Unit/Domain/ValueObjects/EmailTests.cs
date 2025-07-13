using DentalClinic.Domain;

namespace DentalClinic.Tests.Unit.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("test+tag@example.org")]
    [InlineData("name123@test-domain.com")]
    public void Parse_ValidEmail_ShouldCreateInstance(string validEmail)
    {
        // Act
        var email = Email.Parse(validEmail);

        // Assert
        Assert.Equal(validEmail, email.Value);
    }

    [Fact]
    public void Parse_NullEmail_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Email.Parse(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Parse_WhiteSpaceEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Parse(invalidEmail));
    }

    [Theory]
    [InlineData("test.example")]
    [InlineData("test.example@")]
    [InlineData("@example.com")]
    [InlineData("test.example.com")]
    [InlineData("test@@example.com")]
    [InlineData("test@example")]
    [InlineData("test@.com")]
    public void Parse_InvalidEmailFormat_ShouldThrowFormatException(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => Email.Parse(invalidEmail));
    }

    [Fact]
    public void Parse_EmailTooShort_ShouldThrowArgumentException()
    {
        // Arrange
        var shortEmail = "a@b.c"; // 5 characters, minimum is 6

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Parse(shortEmail));
    }

    [Fact]
    public void Parse_EmailTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longEmail = new string('a', 310) + "@example.com"; // > 320 characters

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Parse(longEmail));
    }

    [Fact]
    public void Equality_SameEmails_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Parse("test@example.com");
        var email2 = Email.Parse("test@example.com");

        // Act & Assert
        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
        Assert.False(email1 != email2);
    }

    [Fact]
    public void Equality_DifferentEmails_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = Email.Parse("test1@example.com");
        var email2 = Email.Parse("test2@example.com");

        // Act & Assert
        Assert.NotEqual(email1, email2);
        Assert.False(email1 == email2);
        Assert.True(email1 != email2);
    }
} 
