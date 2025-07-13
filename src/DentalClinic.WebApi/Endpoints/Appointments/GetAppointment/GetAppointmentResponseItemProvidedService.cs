using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Appointments.GetAppointment;

public sealed record GetAppointmentResponseItemProvidedService
{
    public required GuidEntityId<Service> Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
}
