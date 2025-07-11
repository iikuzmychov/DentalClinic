using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Services.AddService;

internal sealed class AddServiceEndpoint : IEndpoint<ServicesEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPost("/", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin)));
    }

    private static async Task<Results<Ok<AddServiceResponse>, Conflict>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromBody] AddServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Services.AnyAsync(service => service.Name == request.Name, cancellationToken))
        {
            return TypedResults.Conflict();
        }

        var service = new Service
        {
            Name = request.Name,
            Price = new Price(request.Price)
        };

        await dbContext.Services.AddAsync(service, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new AddServiceResponse
        {
            Id = service.Id.Value
        });
    }
}
