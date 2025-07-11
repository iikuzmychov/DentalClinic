using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Patients.AddPatient;

internal sealed class AddPatientEndpoint : IEndpoint<PatientsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPost("/", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin), nameof(Role.Receptionist)));
    }

    private static async Task<Results<Ok<AddPatientResponse>, Conflict>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromBody] AddPatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var isEmailOccupied =
            request.Email is not null &&
            await dbContext.Patients.AnyAsync(patient => patient.Email == request.Email, cancellationToken);

        if (isEmailOccupied)
        {
            return TypedResults.Conflict();
        }

        var patient = new Patient
        {
            LastName = request.LastName,
            FirstName = request.FirstName,
            Surname = request.Surname,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Notes = request.Notes
        };

        await dbContext.Patients.AddAsync(patient, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new AddPatientResponse
        {
            Id = patient.Id
        });
    }

}
