using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using DentalClinic.WebApi.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DentalClinic.WebApi.Controllers;

[ApiController]
[Authorize(Roles = nameof(Role.Admin))]
[Route("api/users")]
public sealed class UsersController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ListUsersResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ListUsersResponse> ListAsync(
        [FromQuery] int pageIndex = Constants.DefaultPageIndex,
        [FromQuery] int pageSize = Constants.DefaultPageSize,
        [FromQuery] string? name = null,
        [FromQuery] Role? role = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.LastName)
                .ThenBy(user => user.FirstName)
                    .ThenBy(user => user.Surname)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var lowerInvariantName = name.ToLower();

            query = query.Where(user =>
                user.LastName.ToLower().Contains(lowerInvariantName) ||
                user.FirstName.ToLower().Contains(lowerInvariantName) ||
                (!string.IsNullOrWhiteSpace(user.Surname) && user.Surname!.ToLower().Contains(lowerInvariantName)));
        }

        if (role.HasValue)
        {
            query = query.Where(user => user.Role == role.Value);
        }

        var users = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new ListUsersResponse
        {
            Items = users
                .Select(user => new ListUsersResponseItem
                {
                    Id = user.Id.Value,
                    LastName = user.LastName,
                    FirstName = user.FirstName,
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
    [ProducesResponseType<GetUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
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
            LastName = user.LastName,
            FirstName = user.FirstName,
            Surname = user.Surname,
            Role = user.Role,
            Email = user.Email.Value,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPost]
    [ProducesResponseType<AddUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<Conflict>(StatusCodes.Status409Conflict)]
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
            Id = user.Id.Value
        });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFound>(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok, NotFound>> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.GetByIdOrDefaultAsync(
            new GuidEntityId<User>(id),
            cancellationToken);

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

        return TypedResults.Ok();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok, NotFound>> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var userToDelete = await dbContext.Users.GetByIdOrDefaultAsync(
            new GuidEntityId<User>(id),
            cancellationToken);

        if (userToDelete is null)
        {
            return TypedResults.NotFound();
        }

        dbContext.Users.Remove(userToDelete);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }
}
