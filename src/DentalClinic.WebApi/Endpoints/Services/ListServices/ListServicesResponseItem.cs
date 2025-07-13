using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Services.ListServices;

public sealed record ListServicesResponseItem
{
    public required GuidEntityId<Service> Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
}
