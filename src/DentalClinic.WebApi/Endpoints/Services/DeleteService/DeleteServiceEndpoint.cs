using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using DentalClinic.WebApi.Endpoints.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.WebApi.Endpoints.Users.DeleteService;

internal sealed class DeleteServiceEndpoint : IEndpoint<ServicesEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapDelete("{id:guid}", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin)));
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] GuidEntityId<Service> id,
        CancellationToken cancellationToken = default)
    {
        var serviceToDelete = await dbContext.Services.GetByIdOrDefaultAsync(id, cancellationToken);

        if (serviceToDelete is null)
        {
            return TypedResults.NotFound();
        }

        dbContext.Services.Remove(serviceToDelete);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
