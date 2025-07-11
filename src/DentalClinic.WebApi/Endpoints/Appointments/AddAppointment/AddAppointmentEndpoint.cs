using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DentalClinic.WebApi.Endpoints.Appointments.AddAppointment;

internal sealed class AddAppointmentEndpoint : IEndpoint<AppointmentsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPost("/", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Admin), nameof(Role.Receptionist)));
    }

    private static async Task<Results<Ok<AddAppointmentResponse>, Conflict, NotFound>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromBody] AddAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
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

        var appointment = new Appointment(patient, dentist, request.StartTime, request.Duration);

        await dbContext.Appointments.AddAsync(appointment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new AddAppointmentResponse
        {
            Id = appointment.Id
        });
    }
}
