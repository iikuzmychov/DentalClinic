using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using DentalClinic.WebApi.Models.Requests;
using DentalClinic.WebApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Controllers;

[ApiController]
[Authorize(Roles = nameof(Role.Admin))]
[Route("api/users")]
public sealed class UsersController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ListUsersResponse> ListAsync(
        [FromQuery] int pageIndex = Constants.DefaultPageIndex,
        [FromQuery] int pageSize = Constants.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.FirstName)
                .ThenBy(user => user.LastName)
                    .ThenBy(user => user.Surname)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await dbContext.Users.CountAsync(cancellationToken);
        var totalPagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new ListUsersResponse
        {
            Items = users
                .Select(user => new ListUsersResponseItem
                {
                    Id = user.Id.Value,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Surname = user.Surname,
                    Role = user.Role,
                    Email = user.Email.Value,
                    PhoneNumber = user.PhoneNumber
                })
                .ToList(),
            TotalCount = totalCount,
            TotalPagesCount = totalPagesCount
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<Results<Ok<GetUserResponse>, NotFound>> GetAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .GetByIdOrDefaultAsync(new GuidEntityId<User>(id), cancellationToken);

        if (user is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new GetUserResponse
        {
            Id = user.Id.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Surname = user.Surname,
            Role = user.Role,
            Email = user.Email.Value,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPost]
    public async Task<Results<Ok<AddUserResponse>, Conflict>> AddAsync(
        [FromBody] AddUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(user => user.Email == (object)request.Email, cancellationToken))
        {
            return TypedResults.Conflict();
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
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
            Id = user.Id.Value
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<Results<Ok, NotFound>> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .GetByIdOrDefaultAsync(new GuidEntityId<User>(id), cancellationToken);

        if (user is null)
        {
            return TypedResults.NotFound();
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Surname = request.Surname;
        user.PhoneNumber = request.PhoneNumber;
        
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<Results<Ok, NotFound>> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .GetByIdOrDefaultAsync(new GuidEntityId<User>(id), cancellationToken);

        if (user is null)
        {
            return TypedResults.NotFound();
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }
}
