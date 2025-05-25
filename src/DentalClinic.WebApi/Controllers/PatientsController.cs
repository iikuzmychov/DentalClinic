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
                (!string.IsNullOrWhiteSpace(user.Surname) && user.Surname!.ToLower().Contains(lowerInvariantName)));
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

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetPatientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
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
            LastName = patient.LastName,
            FirstName = patient.FirstName,
            Surname = patient.Surname,
            Email = patient.Email?.Value,
            PhoneNumber = patient.PhoneNumber,
            Notes = patient.Notes
        });
    }

    [HttpPost]
    [ProducesResponseType<AddPatientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<Results<Ok<AddPatientResponse>, Conflict>> AddAsync(
        [FromBody] AddPatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var isEmailOccupied =
            request.Email is not null &&
            await dbContext.Patients.AnyAsync(patient => patient.Email == (object?)request.Email, cancellationToken);

        if (isEmailOccupied)
        {
            return TypedResults.Conflict();
        }

        var patient = new Patient
        {
            LastName = request.LastName,
            FirstName = request.FirstName,
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
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<Results<Ok, NotFound, Conflict>> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var patientToUpdate = await dbContext.Patients.GetByIdOrDefaultAsync(
            new GuidEntityId<Patient>(id),
            cancellationToken);

        if (patientToUpdate is null)
        {
            return TypedResults.NotFound();
        }

        var isEmailOccupied =
            request.Email is not null &&
            await dbContext.Patients
                .Where(patient => patient.Id != patientToUpdate.Id)
                .AnyAsync(patient => patient.Email == (object?)request.Email, cancellationToken);

        if (isEmailOccupied)
        {
            return TypedResults.Conflict();
        }

        patientToUpdate.LastName = request.LastName;
        patientToUpdate.FirstName = request.FirstName;
        patientToUpdate.Surname = request.Surname;
        patientToUpdate.Email = request.Email is null ? null : new Email(request.Email);
        patientToUpdate.PhoneNumber = request.PhoneNumber;
        patientToUpdate.Notes = request.Notes;
        
        dbContext.Patients.Update(patientToUpdate);
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
        var patientToDelete = await dbContext.Patients.GetByIdOrDefaultAsync(
            new GuidEntityId<Patient>(id),
            cancellationToken);

        if (patientToDelete is null)
        {
            return TypedResults.NotFound();
        }

        dbContext.Patients.Remove(patientToDelete);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }
}
