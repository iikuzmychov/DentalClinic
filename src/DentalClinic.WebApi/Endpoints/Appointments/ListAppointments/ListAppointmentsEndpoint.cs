using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Domain.ValueObjects;
using DentalClinic.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Endpoints.Appointments.ListAppointments;

internal sealed class ListAppointmentsEndpoint : IEndpoint<AppointmentsEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group.MapGet("/", HandleAsync);
    }

    private static async Task<ListAppointmentsResponse> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromQuery] DateTime? startDateTime = null,
        [FromQuery] DateTime? endDateTime = null,
        [FromQuery] GuidEntityId<User>? dentistId = null,
        [FromQuery] GuidEntityId<Patient>? patientId = null,
        [FromQuery] AppointmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Dentist)
            .Include(appointment => appointment.Patient)
            .Include(appointment => appointment.ProvidedServices)
            .OrderBy(appointment => appointment.StartTime)
            .AsQueryable();

        if (startDateTime is not null)
        {
            query = query.Where(appointment => appointment.EndTime > startDateTime.Value);
        }

        if (endDateTime is not null)
        {
            query = query.Where(appointment => appointment.StartTime < endDateTime.Value);
        }

        if (dentistId is not null)
        {
            query = query.Where(appointment => appointment.Dentist.Id == dentistId.Value);
        }

        if (patientId is not null)
        {
            query = query.Where(appointment => appointment.Patient.Id == patientId.Value);
        }

        if (status is not null)
        {
            query = query.Where(appointment => appointment.Status == status.Value);
        }

        var appointments = await query.ToListAsync(cancellationToken);

        return new ListAppointmentsResponse
        {
            Items = appointments
                .Select(appointment => new ListAppointmentsResponseItem
                {
                    Id = appointment.Id,
                    Patient = new()
                    {
                        Id = appointment.Patient.Id,
                        LastName = appointment.Patient.LastName,
                        FirstName = appointment.Patient.FirstName,
                        Surname = appointment.Patient.Surname
                    },
                    Dentist = new()
                    {
                        Id = appointment.Dentist.Id,
                        LastName = appointment.Dentist.LastName,
                        FirstName = appointment.Dentist.FirstName,
                        Surname = appointment.Dentist.Surname
                    },
                    Status = appointment.Status,
                    StartTime = appointment.StartTime,
                    Duration = appointment.Duration,
                    ProvidedServices = appointment.ProvidedServices
                        .Select(service => new ListAppointmentsResponseItemProvidedService
                        {
                            Id = service.Id,
                            Name = service.Name
                        })
                        .ToList(),
                })
                .ToList()
        };
    }
}
