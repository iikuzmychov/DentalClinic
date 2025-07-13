using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.ValueObjects;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Services.UpdateService;

internal sealed class UpdateServicesEndpoint : IEndpoint<ServicesEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPut("{id:guid}", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin)));
    }

    private static async Task<Results<NoContent, Conflict, NotFound>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] GuidEntityId<Service> id,
        [FromBody] UpdateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceToUpdate = await dbContext.Services.GetByIdOrDefaultAsync(id, cancellationToken);

        if (serviceToUpdate is null)
        {
            return TypedResults.NotFound();
        }

        var isNameOccupied = await dbContext.Services
            .Where(service => service.Id != serviceToUpdate.Id)
            .AnyAsync(serice => serice.Name == request.Name, cancellationToken);

        if (isNameOccupied)
        {
            return TypedResults.Conflict();
        }

        serviceToUpdate.Name = request.Name;
        serviceToUpdate.Price = new Price(request.Price);

        dbContext.Services.Update(serviceToUpdate);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
