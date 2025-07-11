using DentalClinic.Domain.Enums;
using DentalClinic.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Users.ListUsers;

internal sealed class ListUsersEndpoint : IEndpoint<UsersEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group.MapGet("/", HandleAsync);
    }

    private static async Task<Ok<ListUsersResponse>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
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
                !string.IsNullOrWhiteSpace(user.Surname) && user.Surname!.ToLower().Contains(lowerInvariantName));
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

        return TypedResults.Ok(new ListUsersResponse
        {
            Items = users
                .Select(user => new ListUsersResponseItem
                {
                    Id = user.Id,
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    Surname = user.Surname,
                    Role = user.Role,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                })
                .ToList(),
            TotalCount = totalCount,
            TotalPagesCount = totalPagesCount
        });
    }
}
