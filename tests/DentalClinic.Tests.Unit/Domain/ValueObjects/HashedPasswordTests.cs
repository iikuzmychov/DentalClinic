using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.UserAggregate;

namespace DentalClinic.Tests.Unit.Domain.ValueObjects;

public class HashedPasswordTests
{
    [Fact]
    public void Constructor_ValidHashAndSalt_ShouldCreateInstance()
    {
        // Arrange
        var hash = new byte[64];
        var salt = new byte[64];
        Array.Fill<byte>(hash, 1);
        Array.Fill<byte>(salt, 2);

        // Act
        var hashedPassword = new HashedPassword(hash, salt);

        // Assert
        Assert.Equal(hash, hashedPassword.Hash);
        Assert.Equal(salt, hashedPassword.Salt);
    }

    [Fact]
    public void Constructor_NullHash_ShouldThrowArgumentNullException()
    {
        // Arrange
        var salt = new byte[64];

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HashedPassword(null!, salt));
    }

    [Fact]
    public void Constructor_NullSalt_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hash = new byte[64];

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HashedPassword(hash, null!));
    }

    [Fact]
    public void Constructor_InvalidHashLength_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidHash = new byte[32]; // Should be 64
        var salt = new byte[64];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new HashedPassword(invalidHash, salt));
    }

    [Fact]
    public void Constructor_InvalidSaltLength_ShouldThrowArgumentException()
    {
        // Arrange
        var hash = new byte[64];
        var invalidSalt = new byte[32]; // Should be 64

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new HashedPassword(hash, invalidSalt));
    }

    [Fact]
    public void FromSecurePassword_ValidPassword_ShouldCreateHashedPassword()
    {
        // Arrange
        var securePassword = new SecurePassword("TestPassword123!");

        // Act
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.Equal(64, hashedPassword.Hash.Length);
        Assert.Equal(64, hashedPassword.Salt.Length);
    }

    [Fact]
    public void FromSecurePassword_NullPassword_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => HashedPassword.FromSecurePassword(null!));
    }

    [Fact]
    public void FromSecurePassword_SamePassword_ShouldGenerateDifferentHashes()
    {
        // Arrange
        var password = new SecurePassword("TestPassword123!");

        // Act
        var hashedPassword1 = HashedPassword.FromSecurePassword(password);
        var hashedPassword2 = HashedPassword.FromSecurePassword(password);

        // Assert
        Assert.NotEqual(hashedPassword1.Hash, hashedPassword2.Hash);
        Assert.NotEqual(hashedPassword1.Salt, hashedPassword2.Salt);
    }

    [Fact]
    public void IsMatch_CorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var securePassword = new SecurePassword(password);
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);

        // Act
        var isMatch = hashedPassword.IsMatch(password);

        // Assert
        Assert.True(isMatch);
    }

    [Fact]
    public void IsMatch_IncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var originalPassword = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var securePassword = new SecurePassword(originalPassword);
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);

        // Act
        var isMatch = hashedPassword.IsMatch(wrongPassword);

        // Assert
        Assert.False(isMatch);
    }

    [Fact]
    public void IsMatch_NullPassword_ShouldThrowArgumentNullException()
    {
        // Arrange
        var securePassword = new SecurePassword("TestPassword123!");
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => hashedPassword.IsMatch(null!));
    }

    [Fact]
    public void Equality_SameHashAndSalt_ShouldBeEqual()
    {
        // Arrange
        var hash = new byte[64];
        var salt = new byte[64];
        Array.Fill<byte>(hash, 1);
        Array.Fill<byte>(salt, 2);

        var hashedPassword1 = new HashedPassword(hash, salt);
        var hashedPassword2 = new HashedPassword(hash, salt);

        // Act & Assert
        Assert.Equal(hashedPassword1, hashedPassword2);
    }

    [Fact]
    public void Equality_DifferentHashOrSalt_ShouldNotBeEqual()
    {
        // Arrange
        var hash1 = new byte[64];
        var hash2 = new byte[64];
        var salt = new byte[64];
        Array.Fill<byte>(hash1, 1);
        Array.Fill<byte>(hash2, 2);
        Array.Fill<byte>(salt, 3);

        var hashedPassword1 = new HashedPassword(hash1, salt);
        var hashedPassword2 = new HashedPassword(hash2, salt);

        // Act & Assert
        Assert.NotEqual(hashedPassword1, hashedPassword2);
    }
} 