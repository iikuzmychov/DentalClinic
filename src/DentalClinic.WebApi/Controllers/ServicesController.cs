using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.ServiceAggregate;
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
[Route("api/services")]
public sealed class ServicesController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ListServicesResponse> ListAsync(
        [FromQuery] int pageIndex = Constants.DefaultPageIndex,
        [FromQuery] int pageSize = Constants.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var services = await dbContext.Services
            .AsNoTracking()
            .OrderBy(service => service.Name)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await dbContext.Services.CountAsync(cancellationToken);
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
    public async Task<Results<Ok, NotFound, Conflict>> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        var service = await dbContext.Services
            .AsNoTracking()
            .GetByIdOrDefaultAsync(new GuidEntityId<Service>(id), cancellationToken);

        if (service is null)
        {
            return TypedResults.NotFound();
        }

        var isNameOccupied = await dbContext.Services.AnyAsync(
            serice => serice.Name == request.Name && serice.Id != service.Id,
            cancellationToken);

        if (isNameOccupied)
        {
            return TypedResults.Conflict();
        }

        service.Name = request.Name;
        service.Price = new Price(request.Price);
        
        dbContext.Services.Update(service);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<Results<Ok, NotFound>> DeleteAsync(
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

        dbContext.Services.Remove(service);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }
}
