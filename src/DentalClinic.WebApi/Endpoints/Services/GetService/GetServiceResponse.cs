using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Types;

namespace DentalClinic.WebApi.Endpoints.Services.GetService;

public sealed record GetServiceResponse
{
    public required GuidEntityId<Service> Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
}
