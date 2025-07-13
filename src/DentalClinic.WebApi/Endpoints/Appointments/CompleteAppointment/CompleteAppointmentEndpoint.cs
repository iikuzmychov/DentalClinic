using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.ValueObjects;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Appointments.CompleteAppointment;

internal sealed class CompleteAppointmentEndpoint : IEndpoint<AppointmentsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPost("{id:guid}/complete", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin), nameof(Role.Dentist)));
    }
    private static async Task<Results<NoContent, NotFound, Conflict>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] GuidEntityId<Appointment> id,
        [FromBody] CompleteAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var appointmentToComplete = await dbContext.Appointments
            .Include(appointment => appointment.ProvidedServices)
            .GetByIdOrDefaultAsync(id, cancellationToken);

        if (appointmentToComplete is null)
        {
            return TypedResults.NotFound();
        }

        if (appointmentToComplete.Status is not AppointmentStatus.Scheduled)
        {
            return TypedResults.Conflict();
        }

        var providedServices = await dbContext.Services
            .FilterByIds(request.ProvidedServiceIds)
            .ToListAsync(cancellationToken);

        if (providedServices.Count != request.ProvidedServiceIds.Count)
        {
            return TypedResults.NotFound();
        }

        if (providedServices.Count == 0)
        {
            return TypedResults.Conflict();
        }

        appointmentToComplete.ChangeDuration(request.Duration);
        appointmentToComplete.Complete(providedServices);

        dbContext.Appointments.Update(appointmentToComplete);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
