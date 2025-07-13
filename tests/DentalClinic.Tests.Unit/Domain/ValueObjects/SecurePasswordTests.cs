using DentalClinic.Domain;

namespace DentalClinic.Tests.Unit.Domain.ValueObjects;

public class SecurePasswordTests
{
    [Theory]
    [InlineData("Password123!")]
    [InlineData("MySecure@Pass1")]
    [InlineData("Test#Password1")]
    [InlineData("Strong$Pass123")]
    public void Parse_ValidPassword_ShouldCreateInstance(string validPassword)
    {
        // Act
        var password = SecurePassword.Parse(validPassword);

        // Assert
        Assert.Equal(validPassword, password.Value);
    }

    [Fact]
    public void Parse_NullPassword_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => SecurePassword.Parse(null!));
    }

    [Theory]
    [InlineData("Pass1!")]  // Too short (6 characters)
    [InlineData("Sh0rt!")]  // Too short (6 characters)
    public void Parse_PasswordTooShort_ShouldThrowArgumentException(string shortPassword)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => SecurePassword.Parse(shortPassword));
    }

    [Fact]
    public void Parse_PasswordTooLong_ShouldThrowArgumentException()
    {
        // Arrange - создаем пароль длиной больше 64 символов, но валидный по regex
        var longPassword = "A" + new string('a', 60) + "123!"; // 1 + 60 + 4 = 65 символов

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SecurePassword.Parse(longPassword));
    }

    [Theory]
    [InlineData("password123!")]  // No uppercase
    [InlineData("PASSWORD123!")]  // No lowercase
    [InlineData("Password!")]     // No digits
    [InlineData("Password123")]   // No special characters
    [InlineData("Passw ord123!")] // Contains space (not allowed)
    [InlineData("Password123+")]  // Invalid special character
    public void Parse_InvalidPasswordFormat_ShouldThrowArgumentException(string invalidPassword)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => SecurePassword.Parse(invalidPassword));
    }

    [Fact]
    public void Parse_ValidPasswordWithAllRequiredCharacters_ShouldPass()
    {
        // Arrange
        var validPassword = "TestPassword123!";

        // Act
        var password = SecurePassword.Parse(validPassword);

        // Assert
        Assert.Equal(validPassword, password.Value);
    }

    [Fact]
    public void Equality_SamePasswords_ShouldBeEqual()
    {
        // Arrange
        var password1 = SecurePassword.Parse("TestPassword123!");
        var password2 = SecurePassword.Parse("TestPassword123!");

        // Act & Assert
        Assert.Equal(password1, password2);
        Assert.True(password1 == password2);
        Assert.False(password1 != password2);
    }

    [Fact]
    public void Equality_DifferentPasswords_ShouldNotBeEqual()
    {
        // Arrange
        var password1 = SecurePassword.Parse("TestPassword123!");
        var password2 = SecurePassword.Parse("DifferentPass456#");

        // Act & Assert
        Assert.NotEqual(password1, password2);
        Assert.False(password1 == password2);
        Assert.True(password1 != password2);
    }
} 
