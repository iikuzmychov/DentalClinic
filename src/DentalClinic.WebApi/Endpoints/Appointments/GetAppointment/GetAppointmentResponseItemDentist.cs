using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Types;

namespace DentalClinic.WebApi.Endpoints.Appointments.GetAppointment;

public sealed record GetAppointmentResponseItemDentist
{
    public required GuidEntityId<User> Id { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string? Surname { get; init; }
}

