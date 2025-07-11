using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Patients.UpdatePatient;

internal sealed class UpdatePatientEndpoint : IEndpoint<PatientsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPut("{id:guid}", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin), nameof(Role.Receptionist)));
    }

    private static async Task<Results<NoContent, NotFound, Conflict>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] GuidEntityId<Patient> id,
        [FromBody] UpdatePatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var patientToUpdate = await dbContext.Patients.GetByIdOrDefaultAsync(id, cancellationToken);

        if (patientToUpdate is null)
        {
            return TypedResults.NotFound();
        }

        var isEmailOccupied =
            request.Email is not null &&
            await dbContext.Patients
                .Where(patient => patient.Id != patientToUpdate.Id)
                .AnyAsync(patient => patient.Email == request.Email, cancellationToken);

        if (isEmailOccupied)
        {
            return TypedResults.Conflict();
        }

        patientToUpdate.LastName = request.LastName;
        patientToUpdate.FirstName = request.FirstName;
        patientToUpdate.Surname = request.Surname;
        patientToUpdate.Email = request.Email;
        patientToUpdate.PhoneNumber = request.PhoneNumber;
        patientToUpdate.Notes = request.Notes;

        dbContext.Patients.Update(patientToUpdate);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
