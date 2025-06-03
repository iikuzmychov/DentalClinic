using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;

namespace DentalClinic.Tests.Unit.TestHelpers;

public static class TestDataFactory
{
    public static Patient CreatePatient(
        string firstName = "John", 
        string lastName = "Doe", 
        string? surname = "Middle",
        string? email = "patient@example.com",
        string? phoneNumber = "+1234567890")
    {
        return new Patient
        {
            FirstName = firstName,
            LastName = lastName,
            Surname = surname,
            Email = email != null ? new Email(email) : null,
            PhoneNumber = phoneNumber
        };
    }

    public static User CreateUser(
        string firstName = "John",
        string lastName = "Smith",
        Role role = Role.Dentist,
        string email = "user@example.com",
        string password = "TestPassword123!",
        string? phoneNumber = "+1234567890")
    {
        var userEmail = new Email(email);
        var securePassword = new SecurePassword(password);
        var hashedPassword = HashedPassword.FromSecurePassword(securePassword);

        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            Email = userEmail,
            HashedPassword = hashedPassword,
            PhoneNumber = phoneNumber
        };
    }

    public static User CreateDentist(
        string firstName = "Dr. John",
        string lastName = "Smith",
        string email = "dentist@example.com")
    {
        return CreateUser(firstName, lastName, Role.Dentist, email);
    }

    public static User CreateReceptionist(
        string firstName = "Alice",
        string lastName = "Johnson",
        string email = "receptionist@example.com")
    {
        return CreateUser(firstName, lastName, Role.Receptionist, email);
    }

    public static User CreateAdmin(
        string firstName = "Admin",
        string lastName = "User",
        string email = "admin@example.com")
    {
        return CreateUser(firstName, lastName, Role.Admin, email);
    }

    public static Service CreateService(
        string name = "Dental Cleaning",
        decimal price = 100.00m)
    {
        return new Service
        {
            Name = name,
            Price = new Price(price)
        };
    }

    public static Appointment CreateAppointment(
        Patient? patient = null,
        User? dentist = null,
        DateTime? startTime = null,
        TimeSpan? duration = null)
    {
        patient ??= CreatePatient();
        dentist ??= CreateDentist();
        startTime ??= DateTime.UtcNow.AddDays(1);
        duration ??= TimeSpan.FromHours(1);

        return new Appointment(patient, dentist, startTime.Value, duration.Value);
    }

    public static List<Service> CreateServices(int count = 2)
    {
        var services = new List<Service>();
        for (int i = 0; i < count; i++)
        {
            services.Add(CreateService($"Service {i + 1}", 100.00m + (i * 50)));
        }
        return services;
    }
} 