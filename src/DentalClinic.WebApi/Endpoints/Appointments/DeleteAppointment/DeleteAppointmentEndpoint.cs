using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.ValueObjects;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Appointments.DeleteAppointment;

internal sealed class DeleteAppointmentEndpoint : IEndpoint<AppointmentsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapDelete("{id:guid}", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin), nameof(Role.Receptionist)));
    }
    private static async Task<Results<NoContent, NotFound, Conflict>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] GuidEntityId<Appointment> id,
        CancellationToken cancellationToken = default)
    {
        var appointmentToDelete = await dbContext.Appointments
            .AsNoTracking()
            .GetByIdOrDefaultAsync(id, cancellationToken);

        if (appointmentToDelete is null)
        {
            return TypedResults.NotFound();
        }

        if (appointmentToDelete.Status is AppointmentStatus.Completed or AppointmentStatus.Paid)
        {
            return TypedResults.Conflict();
        }

        dbContext.Appointments.Remove(appointmentToDelete);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
