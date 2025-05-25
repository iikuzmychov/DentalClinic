using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using DentalClinic.WebApi.Models.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Controllers;

[ApiController]
[Authorize(Roles = nameof(Role.Admin))]
[Route("api/patients")]
public sealed class PatientsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ListPatientsResponse>(StatusCodes.Status200OK)]
    public async Task<ListPatientsResponse> ListAsync(
        [FromQuery] int pageIndex = Constants.DefaultPageIndex,
        [FromQuery] int pageSize = Constants.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var patients = await dbContext.Patients
            .AsNoTracking()
            .OrderBy(patient => patient.LastName)
                .ThenBy(patient => patient.FirstName)
                    .ThenBy(patient => patient.Surname)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await dbContext.Patients.CountAsync(cancellationToken);
        var totalPagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new ListPatientsResponse
        {
            Items = patients
                .Select(patient => new ListPatientsResponseItem
                {
                    Id = patient.Id.Value,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    Surname = patient.Surname,
                    Email = patient.Email?.Value,
                    PhoneNumber = patient.PhoneNumber,
                    Notes = patient.Notes
                })
                .ToList(),
            TotalCount = totalCount,
            TotalPagesCount = totalPagesCount
        };
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetPatientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<GetPatientResponse>, NotFound>> GetAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var patient = await dbContext.Patients
            .AsNoTracking()
            .GetByIdOrDefaultAsync(new GuidEntityId<Patient>(id), cancellationToken);

        if (patient is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new GetPatientResponse
        {
            Id = patient.Id.Value,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Surname = patient.Surname,
            Email = patient.Email?.Value,
            PhoneNumber = patient.PhoneNumber,
            Notes = patient.Notes
        });
    }

    [HttpPost]
    [ProducesResponseType<AddPatientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<Results<Ok<AddPatientResponse>, Conflict>> AddAsync(
        [FromBody] AddPatientRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Patients.AnyAsync(patient => patient.Email == (object?)request.Email, cancellationToken))
        {
            return TypedResults.Conflict();
        }

        var patient = new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Surname = request.Surname,
            Email = request.Email is null ? null : new Email(request.Email),
            PhoneNumber = request.PhoneNumber,
            Notes = request.Notes
        };

        await dbContext.Patients.AddAsync(patient, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new AddPatientResponse
        {
            Id = patient.Id.Value
        });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok, NotFound>> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var patient = await dbContext.Patients
            .AsNoTracking()
            .GetByIdOrDefaultAsync(new GuidEntityId<Patient>(id), cancellationToken);

        if (patient is null)
        {
            return TypedResults.NotFound();
        }

        patient.FirstName = request.FirstName;
        patient.LastName = request.LastName;
        patient.Surname = request.Surname;
        patient.Email = request.Email is null ? null : new Email(request.Email);
        patient.PhoneNumber = request.PhoneNumber;
        patient.Notes = request.Notes;
        
        dbContext.Patients.Update(patient);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok, NotFound>> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var patient = await dbContext.Patients
            .AsNoTracking()
            .GetByIdOrDefaultAsync(new GuidEntityId<Patient>(id), cancellationToken);

        if (patient is null)
        {
            return TypedResults.NotFound();
        }

        dbContext.Patients.Remove(patient);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }
}
