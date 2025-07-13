using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Services.AddService;

public sealed record AddServiceResponse
{
    public required GuidEntityId<Service> Id { get; init; }
}
