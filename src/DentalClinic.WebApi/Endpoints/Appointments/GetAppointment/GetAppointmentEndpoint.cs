using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Appointments.GetAppointment;

internal sealed class GetAppointmentEndpoint : IEndpoint<AppointmentsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group.MapGet("{id:guid}", HandleAsync);
    }

    private static async Task<Results<Ok<GetAppointmentResponse>, NotFound>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var appointment = await dbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Dentist)
            .Include(appointment => appointment.Patient)
            .Include(appointment => appointment.ProvidedServices)
            .GetByIdOrDefaultAsync(new GuidEntityId<Appointment>(id), cancellationToken);

        if (appointment is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new GetAppointmentResponse
        {
            Id = appointment.Id.Value,
            Patient = new()
            {
                Id = appointment.Patient.Id.Value,
                LastName = appointment.Patient.LastName,
                FirstName = appointment.Patient.FirstName,
                Surname = appointment.Patient.Surname
            },
            Dentist = new()
            {
                Id = appointment.Dentist.Id.Value,
                LastName = appointment.Dentist.LastName,
                FirstName = appointment.Dentist.FirstName,
                Surname = appointment.Dentist.Surname
            },
            Status = appointment.Status,
            StartTime = appointment.StartTime,
            Duration = appointment.Duration,
            ProvidedServices = appointment.ProvidedServices
                .Select(service => new GetAppointmentResponseItemProvidedService
                {
                    Id = service.Id.Value,
                    Name = service.Name,
                    Price = service.Price.Value
                })
                .ToList()
        });
    }
}
