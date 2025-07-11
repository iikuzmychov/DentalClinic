using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Users.GetUser;

internal sealed class GetUserEndpoint : IEndpoint<UsersEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group.MapGet("{id:guid}", HandleAsync);
    }

    private static async Task<Results<Ok<GetUserResponse>, NotFound>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] GuidEntityId<User> id,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .GetByIdOrDefaultAsync(id, cancellationToken);

        if (user is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new GetUserResponse
        {
            Id = user.Id,
            LastName = user.LastName,
            FirstName = user.FirstName,
            Surname = user.Surname,
            Role = user.Role,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        });
    }
}
