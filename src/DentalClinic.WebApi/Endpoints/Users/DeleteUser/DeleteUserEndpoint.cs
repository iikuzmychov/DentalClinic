using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.ValueObjects;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.WebApi.Endpoints.Users.DeleteUser;

internal sealed class DeleteUserEndpoint : IEndpoint<UsersEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapDelete("{id:guid}", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin)));
    }

    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] GuidEntityId<User> id,
        CancellationToken cancellationToken = default)
    {
        var userToDelete = await dbContext.Users.GetByIdOrDefaultAsync(id, cancellationToken);

        if (userToDelete is null)
        {
            return TypedResults.NotFound();
        }

        dbContext.Users.Remove(userToDelete);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
