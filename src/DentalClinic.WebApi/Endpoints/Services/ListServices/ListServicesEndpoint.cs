using DentalClinic.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Services.ListServices;

internal sealed class ListServicesEndpoint : IEndpoint<ServicesEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group.MapGet("/", HandleAsync);
    }

    private static async Task<Ok<ListServicesResponse>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromQuery] int pageIndex = Constants.DefaultPageIndex,
        [FromQuery] int pageSize = Constants.DefaultPageSize,
        [FromQuery] string? name = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Services
            .AsNoTracking()
            .OrderBy(service => service.Name)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(service => service.Name.ToLower().Contains(name.ToLower()));
        }

        var services = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

        return TypedResults.Ok(new ListServicesResponse
        {
            Items = services
                .Select(service => new ListServicesResponseItem
                {
                    Id = service.Id.Value,
                    Name = service.Name,
                    Price = service.Price.Value,
                })
                .ToList(),
            TotalCount = totalCount,
            TotalPagesCount = totalPagesCount
        });
    }
}
