using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Types;

namespace DentalClinic.WebApi.Endpoints.Appointments.CompleteAppointment;

public sealed record CompleteAppointmentRequest
{
    public required TimeSpan Duration { get; init; }
    public required List<GuidEntityId<Service>> ProvidedServiceIds { get; init; }
}
