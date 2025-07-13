using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Appointments.ListAppointments;

public sealed record ListAppointmentsResponseItemProvidedService
{
    public required GuidEntityId<Service> Id { get; init; }
    public required string Name { get; init; }
}
