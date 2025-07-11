using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.WebApi.Endpoints.Appointments.PayAppointment;

internal sealed class PayAppointmentEndpoint : IEndpoint<AppointmentsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPost("{id:guid}/pay", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin), nameof(Role.Receptionist)));
    }

    private static async Task<Results<NoContent, NotFound, Conflict>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var appointmentToPay = await dbContext.Appointments.GetByIdOrDefaultAsync(
            new GuidEntityId<Appointment>(id),
            cancellationToken);

        if (appointmentToPay is null)
        {
            return TypedResults.NotFound();
        }

        if (appointmentToPay.Status is not AppointmentStatus.Completed)
        {
            return TypedResults.Conflict();
        }

        appointmentToPay.Pay();

        dbContext.Appointments.Update(appointmentToPay);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
