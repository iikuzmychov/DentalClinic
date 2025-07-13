using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using EntityFrameworkCore.Projectables;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.Domain.Aggregates.AppointmentAggregate;

public sealed class Appointment : IIdentifiable<GuidEntityId<Appointment>>
{
    private readonly List<Service> _providedServices;

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public GuidEntityId<Appointment> Id { get; init; } = GuidEntityId<Appointment>.New();

    public Patient Patient { get; private set; }
    public User Dentist { get; private set; }
    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Scheduled;
    public DateTime StartTime { get; private set; }
    public TimeSpan Duration { get; private set; }
    
    [Projectable]
    public DateTime EndTime => StartTime + Duration;

    public IReadOnlyCollection<Service> ProvidedServices => _providedServices.AsReadOnly();

    public Appointment(Patient patient, User dentist, DateTime startTime, TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(patient, nameof(patient));
        ArgumentNullException.ThrowIfNull(dentist, nameof(dentist));
        
        if (dentist.Role is not Role.Dentist)
        {
            throw new ArgumentException($"Dentist must have the role of {Role.Dentist}.", nameof(dentist));
        }

        if (startTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException($"Start time must be in UTC.", nameof(startTime));
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be greater than zero.");
        }

        Patient = patient;
        Dentist = dentist;
        StartTime = startTime;
        Duration = duration;
        _providedServices = [];
    }

    [Obsolete("This constructor is for EF Core only.")]
    private Appointment()
    {
        Patient = default!;
        Dentist = default!;
        _providedServices = default!;
    }

    public void ChangeDentist(User dentist)
    {
        ArgumentNullException.ThrowIfNull(dentist, nameof(dentist));
        
        if (dentist.Role is not Role.Dentist)
        {
            throw new ArgumentException($"Dentist must have the role of {Role.Dentist}.", nameof(dentist));
        }

        if (Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled appointments can have their dentist changed.");
        }

        Dentist = dentist;
    }

    public void ChangePatient(Patient patient)
    {
        ArgumentNullException.ThrowIfNull(patient, nameof(patient));

        if (Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled appointments can have their patient changed.");
        }

        Patient = patient;
    }

    public void ChangeStartTime(DateTime startTime)
    {
        if (startTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException($"Date and time must be in UTC.", nameof(startTime));
        }

        if (Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled appointments can have their date and time changed.");
        }

        StartTime = startTime;
    }

    public void ChangeDuration(TimeSpan duration)
    {
        if (Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled appointments can have their duration changed.");
        }

        Duration = duration;
    }

    public void Cancel()
    {
        if (Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled appointments can be cancelled.");
        }

        Status = AppointmentStatus.Cancelled;
    }

    public void Complete(ICollection<Service> providedServices)
    {
        ArgumentNullException.ThrowIfNull(providedServices);

        if (providedServices.Count == 0)
        {
            throw new ArgumentException("At least one provided service must be specified.", nameof(providedServices));
        }

        if (providedServices.Any(service => service is null))
        {
            throw new ArgumentException("Provided services cannot contain null values.", nameof(providedServices));
        }

        if (Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled appointments can be complited.");
        }

        _providedServices.AddRange(providedServices);
        Status = AppointmentStatus.Completed;
    }

    public void Pay()
    {
        if (Status != AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Only completed appointments can be paid.");
        }

        Status = AppointmentStatus.Paid;
    }
}
