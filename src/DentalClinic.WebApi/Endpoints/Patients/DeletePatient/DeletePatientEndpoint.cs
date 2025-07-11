using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.WebApi.Endpoints.Patients.DeletePatient;

internal sealed class DeletePatientEndpoint : IEndpoint<PatientsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group.MapDelete("{id:guid}", HandleAsync);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(Role.Admin))]
    private static async Task<Results<NoContent, NotFound>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
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

        return TypedResults.NoContent();
    }
}
