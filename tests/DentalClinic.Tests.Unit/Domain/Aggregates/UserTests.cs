using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;

namespace DentalClinic.Tests.Unit.Domain.Aggregates;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldGenerateNewId()
    {
        // Arrange & Act
        var user = CreateValidUser();

        // Assert
        Assert.NotEqual(default, user.Id);
        Assert.NotEqual(Guid.Empty, user.Id.Value);
    }

    [Fact]
    public void FirstName_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var user = CreateValidUser();
        var newFirstName = "NewFirstName";

        // Act
        user.FirstName = newFirstName;

        // Assert
        Assert.Equal(newFirstName, user.FirstName);
    }

    [Fact]
    public void FirstName_Null_ShouldThrowArgumentNullException()
    {
        // Arrange
        var user = CreateValidUser();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.FirstName = null!);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void FirstName_WhiteSpace_ShouldThrowArgumentException(string invalidFirstName)
    {
        // Arrange
        var user = CreateValidUser();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.FirstName = invalidFirstName);
    }

    [Fact]
    public void LastName_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var user = CreateValidUser();
        var newLastName = "NewLastName";

        // Act
        user.LastName = newLastName;

        // Assert
        Assert.Equal(newLastName, user.LastName);
    }

    [Fact]
    public void LastName_Null_ShouldThrowArgumentNullException()
    {
        // Arrange
        var user = CreateValidUser();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.LastName = null!);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void LastName_WhiteSpace_ShouldThrowArgumentException(string invalidLastName)
    {
        // Arrange
        var user = CreateValidUser();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.LastName = invalidLastName);
    }

    [Fact]
    public void Surname_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var user = CreateValidUser();
        var newSurname = "NewSurname";

        // Act
        user.Surname = newSurname;

        // Assert
        Assert.Equal(newSurname, user.Surname);
    }

    [Fact]
    public void Surname_Null_ShouldSetToNull()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.Surname = null;

        // Assert
        Assert.Null(user.Surname);
    }

    [Theory]
    [InlineData(Role.Admin)]
    [InlineData(Role.Dentist)]
    [InlineData(Role.Receptionist)]
    public void Role_ValidEnumValue_ShouldSetProperty(Role validRole)
    {
        // Arrange & Act
        var user = CreateValidUserWithRole(validRole);

        // Assert
        Assert.Equal(validRole, user.Role);
    }

    [Fact]
    public void Role_InvalidEnumValue_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var invalidRole = (Role)999;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateValidUserWithRole(invalidRole));
    }

    [Fact]
    public void Email_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var validEmail = Email.Parse("test@example.com");

        // Act
        var user = CreateValidUserWithEmail(validEmail);

        // Assert
        Assert.Equal(validEmail, user.Email);
    }

    [Fact]
    public void Email_Null_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CreateValidUserWithEmail(null!));
    }

    [Fact]
    public void HashedPassword_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var securePassword = SecurePassword.Parse("TestPassword123!");
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);
        var user = CreateValidUser();

        // Act
        user.HashedPassword = hashedPassword;

        // Assert
        Assert.Equal(hashedPassword, user.HashedPassword);
    }

    [Fact]
    public void HashedPassword_Null_ShouldThrowArgumentNullException()
    {
        // Arrange
        var user = CreateValidUser();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.HashedPassword = null!);
    }

    [Fact]
    public void PhoneNumber_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var user = CreateValidUser();
        var newPhoneNumber = "+1234567890";

        // Act
        user.PhoneNumber = newPhoneNumber;

        // Assert
        Assert.Equal(newPhoneNumber, user.PhoneNumber);
    }

    [Fact]
    public void PhoneNumber_Null_ShouldSetToNull()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        user.PhoneNumber = null;

        // Assert
        Assert.Null(user.PhoneNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void PhoneNumber_WhiteSpace_ShouldThrowArgumentException(string invalidPhoneNumber)
    {
        // Arrange
        var user = CreateValidUser();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => user.PhoneNumber = invalidPhoneNumber);
    }

    [Fact]
    public void TwoUsers_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var user1 = CreateValidUser();
        var user2 = CreateValidUser();

        // Act & Assert
        Assert.NotEqual(user1.Id, user2.Id);
    }

    private static User CreateValidUser()
    {
        var email = Email.Parse("test@example.com");
        var securePassword = SecurePassword.Parse("TestPassword123!");
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);

        return new User
        {
            FirstName = "John",
            LastName = "Doe",
            Role = Role.Dentist,
            Email = email,
            HashedPassword = hashedPassword
        };
    }

    private static User CreateValidUserWithRole(Role role)
    {
        var email = Email.Parse("test@example.com");
        var securePassword = SecurePassword.Parse("TestPassword123!");
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);

        return new User
        {
            FirstName = "John",
            LastName = "Doe",
            Role = role,
            Email = email,
            HashedPassword = hashedPassword
        };
    }

    private static User CreateValidUserWithEmail(Email email)
    {
        var securePassword = SecurePassword.Parse("TestPassword123!");
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);

        return new User
        {
            FirstName = "John",
            LastName = "Doe",
            Role = Role.Dentist,
            Email = email,
            HashedPassword = hashedPassword
        };
    }
} 