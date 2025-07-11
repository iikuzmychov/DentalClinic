using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.WebApi.Endpoints.Appointments.CancelAppointment;

internal sealed class CancelAppointmentEndpoint : IEndpoint<AppointmentsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPost("{id:guid}/cancel", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin), nameof(Role.Receptionist)));
    }

    private static async Task<Results<NoContent, NotFound, Conflict>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] GuidEntityId<Appointment> id,
        CancellationToken cancellationToken = default)
    {
        var appointmentToCancel = await dbContext.Appointments.GetByIdOrDefaultAsync(id, cancellationToken);

        if (appointmentToCancel is null)
        {
            return TypedResults.NotFound();
        }

        if (appointmentToCancel.Status is not AppointmentStatus.Scheduled)
        {
            return TypedResults.Conflict();
        }

        appointmentToCancel.Cancel();

        dbContext.Appointments.Update(appointmentToCancel);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
