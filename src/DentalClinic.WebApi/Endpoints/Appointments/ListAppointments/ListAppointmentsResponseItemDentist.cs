using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Appointments.ListAppointments;

public sealed record ListAppointmentsResponseItemDentist
{
    public required GuidEntityId<User> Id { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string? Surname { get; init; }
}
