using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.ValueObjects;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.WebApi.Endpoints.Users.UpdateUser;

internal sealed class UpdateUserEndpoint : IEndpoint<UsersEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPut("{id:guid}", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Receptionist), nameof(Role.Admin)));
    }

    private static async Task<Results<NoContent, NotFound>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] GuidEntityId<User> id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.GetByIdOrDefaultAsync(id, cancellationToken);

        if (user is null)
        {
            return TypedResults.NotFound();
        }

        user.LastName = request.LastName;
        user.FirstName = request.FirstName;
        user.Surname = request.Surname;
        user.PhoneNumber = request.PhoneNumber;

        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
