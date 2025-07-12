using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Types;

namespace DentalClinic.WebApi.Endpoints.Appointments.ListAppointments;

public sealed record ListAppointmentsResponseItemPatient
{
    public required GuidEntityId<Patient> Id { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string? Surname { get; init; }
}
