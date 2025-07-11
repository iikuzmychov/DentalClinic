using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Users.AddUser;

internal sealed class AddUserEndpoint : IEndpoint<UsersEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPost("/", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Receptionist), nameof(Role.Admin)));
    }

    private static async Task<Results<Ok<AddUserResponse>, Conflict>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromBody] AddUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(user => user.Email == (object)request.Email, cancellationToken))
        {
            return TypedResults.Conflict();
        }

        var user = new User
        {
            LastName = request.LastName,
            FirstName = request.FirstName,
            Surname = request.Surname,
            Role = request.Role,
            Email = new Email(request.Email),
            PhoneNumber = request.PhoneNumber,
            HashedPassword = HashedPassword.FromSecurePassword(new SecurePassword(request.Password))
        };

        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new AddUserResponse
        {
            Id = user.Id
        });
    }
}
