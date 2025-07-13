using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.PatientAggregate;

namespace DentalClinic.Tests.Unit.Domain.Aggregates;

public class PatientTests
{
    [Fact]
    public void Constructor_ShouldGenerateNewId()
    {
        // Arrange & Act
        var patient = CreateValidPatient();

        // Assert
        Assert.NotEqual(default, patient.Id);
        Assert.NotEqual(Guid.Empty, patient.Id.Value);
    }

    [Fact]
    public void FirstName_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var patient = CreateValidPatient();
        var newFirstName = "NewFirstName";

        // Act
        patient.FirstName = newFirstName;

        // Assert
        Assert.Equal(newFirstName, patient.FirstName);
    }

    [Fact]
    public void FirstName_Null_ShouldThrowArgumentNullException()
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => patient.FirstName = null!);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void FirstName_WhiteSpace_ShouldThrowArgumentException(string invalidFirstName)
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => patient.FirstName = invalidFirstName);
    }

    [Fact]
    public void LastName_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var patient = CreateValidPatient();
        var newLastName = "NewLastName";

        // Act
        patient.LastName = newLastName;

        // Assert
        Assert.Equal(newLastName, patient.LastName);
    }

    [Fact]
    public void LastName_Null_ShouldThrowArgumentNullException()
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => patient.LastName = null!);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void LastName_WhiteSpace_ShouldThrowArgumentException(string invalidLastName)
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => patient.LastName = invalidLastName);
    }

    [Fact]
    public void Surname_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var patient = CreateValidPatient();
        var newSurname = "NewSurname";

        // Act
        patient.Surname = newSurname;

        // Assert
        Assert.Equal(newSurname, patient.Surname);
    }

    [Fact]
    public void Email_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var patient = CreateValidPatient();
        var newEmail = Email.Parse("new@example.com");

        // Act
        patient.Email = newEmail;

        // Assert
        Assert.Equal(newEmail, patient.Email);
    }

    [Fact]
    public void Email_Null_ShouldSetToNull()
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act
        patient.Email = null;

        // Assert
        Assert.Null(patient.Email);
    }

    [Fact]
    public void PhoneNumber_ValidValue_ShouldSetProperty()
    {
        // Arrange
        var patient = CreateValidPatient();
        var newPhoneNumber = "+1234567890";

        // Act
        patient.PhoneNumber = newPhoneNumber;

        // Assert
        Assert.Equal(newPhoneNumber, patient.PhoneNumber);
    }

    [Fact]
    public void PhoneNumber_Null_ShouldSetToNull()
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act
        patient.PhoneNumber = null;

        // Assert
        Assert.Null(patient.PhoneNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void PhoneNumber_WhiteSpace_ShouldThrowArgumentException(string invalidPhoneNumber)
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => patient.PhoneNumber = invalidPhoneNumber);
    }

    [Fact]
    public void Notes_AnyValue_ShouldSetProperty()
    {
        // Arrange
        var patient = CreateValidPatient();
        var notes = "Some notes about the patient";

        // Act
        patient.Notes = notes;

        // Assert
        Assert.Equal(notes, patient.Notes);
    }

    [Fact]
    public void Notes_Null_ShouldSetToNull()
    {
        // Arrange
        var patient = CreateValidPatient();

        // Act
        patient.Notes = null;

        // Assert
        Assert.Null(patient.Notes);
    }

    [Fact]
    public void TwoPatients_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var patient1 = CreateValidPatient();
        var patient2 = CreateValidPatient();

        // Act & Assert
        Assert.NotEqual(patient1.Id, patient2.Id);
    }

    private static Patient CreateValidPatient()
    {
        return new Patient
        {
            FirstName = "John",
            LastName = "Doe",
            Surname = "Middle"
        };
    }
} 
