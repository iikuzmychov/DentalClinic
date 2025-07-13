using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.ValueObjects;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Services.GetService;

internal sealed class GetServiceEndpoint : IEndpoint<ServicesEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group.MapGet("{id:guid}", HandleAsync);
    }

    private static async Task<Results<Ok<GetServiceResponse>, NotFound>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] GuidEntityId<Service> id,
        CancellationToken cancellationToken = default)
    {
        var service = await dbContext.Services
            .AsNoTracking()
            .GetByIdOrDefaultAsync(id, cancellationToken);

        if (service is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new GetServiceResponse
        {
            Id = service.Id,
            Name = service.Name,
            Price = service.Price.Value
        });
    }
}
