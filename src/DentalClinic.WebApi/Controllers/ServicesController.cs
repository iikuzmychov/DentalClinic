using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using DentalClinic.WebApi.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Controllers;

[ApiController]
[Route("api/services")]
public sealed class ServicesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ListServicesResponse>(StatusCodes.Status200OK)]
    public async Task<ListServicesResponse> ListAsync(
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

        return new ListServicesResponse
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
        };
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetServiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<GetServiceResponse>, NotFound>> GetAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var service = await dbContext.Services
            .AsNoTracking()
            .GetByIdOrDefaultAsync(new GuidEntityId<Service>(id), cancellationToken);

        if (service is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new GetServiceResponse
        {
            Id = service.Id.Value,
            Name = service.Name,
            Price = service.Price.Value
        });
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    [ProducesResponseType<AddServiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<Results<Ok<AddServiceResponse>, Conflict>> AddAsync(
        [FromBody] AddServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Services.AnyAsync(service => service.Name == request.Name, cancellationToken))
        {
            return TypedResults.Conflict();
        }

        var service = new Service
        {
            Name = request.Name,
            Price = new Price(request.Price)
        };

        await dbContext.Services.AddAsync(service, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new AddServiceResponse
        {
            Id = service.Id.Value
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(Role.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<Results<NoContent, NotFound, Conflict>> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var serviceToUpdate = await dbContext.Services.GetByIdOrDefaultAsync(
            new GuidEntityId<Service>(id),
            cancellationToken);

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

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(Role.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, NotFound>> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var serviceToDelete = await dbContext.Services.GetByIdOrDefaultAsync(
            new GuidEntityId<Service>(id),
            cancellationToken);

        if (serviceToDelete is null)
        {
            return TypedResults.NotFound();
        }

        dbContext.Services.Remove(serviceToDelete);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
