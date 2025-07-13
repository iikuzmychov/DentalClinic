using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Appointments.AddAppointment;

public sealed record AddAppointmentRequest
{
    public required GuidEntityId<Patient> PatientId { get; init; }
    public required GuidEntityId<User> DentistId { get; init; }
    public required DateTime StartTime { get; init; }
    public required TimeSpan Duration { get; init; }
}
