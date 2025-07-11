using DentalClinic.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Patients.ListPatients;

internal sealed class ListPatientsEndpoint : IEndpoint<PatientsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group.MapGet("/", HandleAsync);
    }

    private static async Task<ListPatientsResponse> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromQuery] int pageIndex = Constants.DefaultPageIndex,
        [FromQuery] int pageSize = Constants.DefaultPageSize,
        [FromQuery] string? name = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Patients
            .AsNoTracking()
            .OrderBy(patient => patient.LastName)
                .ThenBy(patient => patient.FirstName)
                    .ThenBy(patient => patient.Surname)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var lowerInvariantName = name.ToLower();

            query = query.Where(user =>
                user.LastName.ToLower().Contains(lowerInvariantName) ||
                user.FirstName.ToLower().Contains(lowerInvariantName) ||
                !string.IsNullOrWhiteSpace(user.Surname) && user.Surname!.ToLower().Contains(lowerInvariantName));
        }

        var patients = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new ListPatientsResponse
        {
            Items = patients
                .Select(patient => new ListPatientsResponseItem
                {
                    Id = patient.Id.Value,
                    LastName = patient.LastName,
                    FirstName = patient.FirstName,
                    Surname = patient.Surname,
                    Email = patient.Email?.Value,
                    PhoneNumber = patient.PhoneNumber
                })
                .ToList(),
            TotalCount = totalCount,
            TotalPagesCount = totalPagesCount
        };
    }
}
