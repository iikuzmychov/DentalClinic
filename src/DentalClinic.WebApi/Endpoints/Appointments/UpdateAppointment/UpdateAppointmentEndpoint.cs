using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.WebApi.Endpoints.Appointments.UpdateAppointment;

internal sealed class UpdateAppointmentEndpoint : IEndpoint<AppointmentsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPut("{id:guid}", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin), nameof(Role.Receptionist)));
    }

    private static async Task<Results<NoContent, NotFound, Conflict>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] GuidEntityId<Appointment> id,
        [FromBody] UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var appointmentToUpdate = await dbContext.Appointments.GetByIdOrDefaultAsync(id, cancellationToken);

        if (appointmentToUpdate is null)
        {
            return TypedResults.NotFound();
        }

        var patient = await dbContext.Patients.GetByIdOrDefaultAsync(request.PatientId, cancellationToken);

        if (patient is null)
        {
            return TypedResults.NotFound();
        }

        var dentist = await dbContext.Dentists.GetByIdOrDefaultAsync(request.DentistId, cancellationToken);

        if (dentist is null)
        {
            return TypedResults.NotFound();
        }

        appointmentToUpdate.ChangePatient(patient);
        appointmentToUpdate.ChangeDentist(dentist);
        appointmentToUpdate.ChangeStartTime(request.StartTime);
        appointmentToUpdate.ChangeDuration(request.Duration);

        dbContext.Appointments.Update(appointmentToUpdate);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
