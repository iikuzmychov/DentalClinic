using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;

namespace DentalClinic.Tests.Unit.Domain.Aggregates;

public class AppointmentTests
{
    private readonly Patient _validPatient;
    private readonly User _validDentist;
    private readonly User _validReceptionist;
    private readonly DateTime _validStartTime;
    private readonly TimeSpan _validDuration;

    public AppointmentTests()
    {
        _validPatient = CreateValidPatient();
        _validDentist = CreateValidDentist();
        _validReceptionist = CreateValidReceptionist();
        _validStartTime = DateTime.UtcNow.AddDays(1);
        _validDuration = TimeSpan.FromHours(1);
    }

    [Fact]
    public void Constructor_ValidParameters_ShouldCreateAppointment()
    {
        // Act
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);

        // Assert
        Assert.NotEqual(default, appointment.Id);
        Assert.Equal(_validPatient, appointment.Patient);
        Assert.Equal(_validDentist, appointment.Dentist);
        Assert.Equal(_validStartTime, appointment.StartTime);
        Assert.Equal(_validDuration, appointment.Duration);
        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        Assert.Empty(appointment.ProvidedServices);
    }

    [Fact]
    public void Constructor_NullPatient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Appointment(null!, _validDentist, _validStartTime, _validDuration));
    }

    [Fact]
    public void Constructor_NullDentist_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new Appointment(_validPatient, null!, _validStartTime, _validDuration));
    }

    [Fact]
    public void Constructor_NonDentistUser_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Appointment(_validPatient, _validReceptionist, _validStartTime, _validDuration));
    }

    [Fact]
    public void Constructor_NonUtcStartTime_ShouldThrowArgumentException()
    {
        // Arrange
        var localStartTime = DateTime.Now.AddDays(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new Appointment(_validPatient, _validDentist, localStartTime, _validDuration));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-3600)]
    public void Constructor_InvalidDuration_ShouldThrowArgumentOutOfRangeException(int seconds)
    {
        // Arrange
        var invalidDuration = TimeSpan.FromSeconds(seconds);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new Appointment(_validPatient, _validDentist, _validStartTime, invalidDuration));
    }

    [Fact]
    public void EndTime_ShouldReturnStartTimePlusDuration()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        var expectedEndTime = _validStartTime + _validDuration;

        // Act
        var endTime = appointment.EndTime;

        // Assert
        Assert.Equal(expectedEndTime, endTime);
    }

    [Fact]
    public void ChangeDentist_ValidDentist_ShouldUpdateDentist()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        var newDentist = CreateValidDentistWithEmail("newdentist@example.com");

        // Act
        appointment.ChangeDentist(newDentist);

        // Assert
        Assert.Equal(newDentist, appointment.Dentist);
    }

    [Fact]
    public void ChangeDentist_NullDentist_ShouldThrowArgumentNullException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => appointment.ChangeDentist(null!));
    }

    [Fact]
    public void ChangeDentist_NonDentistUser_ShouldThrowArgumentException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => appointment.ChangeDentist(_validReceptionist));
    }

    [Fact]
    public void ChangeDentist_NonScheduledAppointment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        appointment.Cancel();
        var newDentist = CreateValidDentistWithEmail("newdentist@example.com");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => appointment.ChangeDentist(newDentist));
    }

    [Fact]
    public void ChangePatient_ValidPatient_ShouldUpdatePatient()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        var newPatient = CreateValidPatientWithEmail("newpatient@example.com");

        // Act
        appointment.ChangePatient(newPatient);

        // Assert
        Assert.Equal(newPatient, appointment.Patient);
    }

    [Fact]
    public void ChangePatient_NullPatient_ShouldThrowArgumentNullException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => appointment.ChangePatient(null!));
    }

    [Fact]
    public void ChangePatient_NonScheduledAppointment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        appointment.Cancel();
        var newPatient = CreateValidPatientWithEmail("newpatient@example.com");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => appointment.ChangePatient(newPatient));
    }

    [Fact]
    public void ChangeStartTime_ValidUtcTime_ShouldUpdateStartTime()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        var newStartTime = DateTime.UtcNow.AddDays(2);

        // Act
        appointment.ChangeStartTime(newStartTime);

        // Assert
        Assert.Equal(newStartTime, appointment.StartTime);
    }

    [Fact]
    public void ChangeStartTime_NonUtcTime_ShouldThrowArgumentException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        var localTime = DateTime.Now.AddDays(2);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => appointment.ChangeStartTime(localTime));
    }

    [Fact]
    public void ChangeStartTime_NonScheduledAppointment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        appointment.Cancel();
        var newStartTime = DateTime.UtcNow.AddDays(2);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => appointment.ChangeStartTime(newStartTime));
    }

    [Fact]
    public void ChangeDuration_ValidDuration_ShouldUpdateDuration()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        var newDuration = TimeSpan.FromHours(2);

        // Act
        appointment.ChangeDuration(newDuration);

        // Assert
        Assert.Equal(newDuration, appointment.Duration);
    }

    [Fact]
    public void ChangeDuration_NonScheduledAppointment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        appointment.Cancel();
        var newDuration = TimeSpan.FromHours(2);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => appointment.ChangeDuration(newDuration));
    }

    [Fact]
    public void Cancel_ScheduledAppointment_ShouldChangeToCancelled()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);

        // Act
        appointment.Cancel();

        // Assert
        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public void Cancel_NonScheduledAppointment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        appointment.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => appointment.Cancel());
    }

    [Fact]
    public void Complete_ValidServices_ShouldCompleteAppointment()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        var services = new List<Service>
        {
            CreateValidService("Dental Cleaning", 100.00m),
            CreateValidService("X-Ray", 50.00m)
        };

        // Act
        appointment.Complete(services);

        // Assert
        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
        Assert.Equal(2, appointment.ProvidedServices.Count);
        Assert.Contains(services[0], appointment.ProvidedServices);
        Assert.Contains(services[1], appointment.ProvidedServices);
    }

    [Fact]
    public void Complete_NullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => appointment.Complete(null!));
    }

    [Fact]
    public void Complete_EmptyServices_ShouldThrowArgumentException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        var emptyServices = new List<Service>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => appointment.Complete(emptyServices));
    }

    [Fact]
    public void Complete_ServicesWithNull_ShouldThrowArgumentException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        var servicesWithNull = new List<Service?> { CreateValidService("Test", 100m), null };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => appointment.Complete(servicesWithNull!));
    }

    [Fact]
    public void Complete_NonScheduledAppointment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        appointment.Cancel();
        var services = new List<Service> { CreateValidService("Test", 100m) };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => appointment.Complete(services));
    }

    [Fact]
    public void Pay_CompletedAppointment_ShouldChangeToPaid()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);
        var services = new List<Service> { CreateValidService("Test", 100m) };
        appointment.Complete(services);

        // Act
        appointment.Pay();

        // Assert
        Assert.Equal(AppointmentStatus.Paid, appointment.Status);
    }

    [Fact]
    public void Pay_NonCompletedAppointment_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = new Appointment(_validPatient, _validDentist, _validStartTime, _validDuration);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => appointment.Pay());
    }

    private static Patient CreateValidPatient()
    {
        return new Patient
        {
            FirstName = "John",
            LastName = "Doe",
            Surname = "Middle",
            Email = new Email("patient@example.com")
        };
    }

    private static Patient CreateValidPatientWithEmail(string email)
    {
        return new Patient
        {
            FirstName = "Jane",
            LastName = "Smith",
            Surname = "Middle",
            Email = new Email(email)
        };
    }

    private static User CreateValidDentist()
    {
        var email = new Email("dentist@example.com");
        var securePassword = new SecurePassword("TestPassword123!");
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);

        return new User
        {
            FirstName = "Dr. John",
            LastName = "Smith",
            Role = Role.Dentist,
            Email = email,
            HashedPassword = hashedPassword
        };
    }

    private static User CreateValidDentistWithEmail(string emailAddress)
    {
        var email = new Email(emailAddress);
        var securePassword = new SecurePassword("TestPassword123!");
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);

        return new User
        {
            FirstName = "Dr. Jane",
            LastName = "Brown",
            Role = Role.Dentist,
            Email = email,
            HashedPassword = hashedPassword
        };
    }

    private static User CreateValidReceptionist()
    {
        var email = new Email("receptionist@example.com");
        var securePassword = new SecurePassword("TestPassword123!");
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);

        return new User
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Role = Role.Receptionist,
            Email = email,
            HashedPassword = hashedPassword
        };
    }

    private static Service CreateValidService(string name, decimal priceValue)
    {
        return new Service
        {
            Name = name,
            Price = new Price(priceValue)
        };
    }
} 