using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Patients.GetPatient;

internal sealed class GetPatientEndpoint : IEndpoint<PatientsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group.MapGet("{id:guid}", HandleAsync);
    }

    private static async Task<Results<Ok<GetPatientResponse>, NotFound>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
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
}
