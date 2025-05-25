using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;
using DentalClinic.Infrastructure;
using DentalClinic.Infrastructure.Extensions;
using DentalClinic.WebApi.Models.Appointments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.WebApi.Controllers;

[ApiController]
[Route("api/appointments")]
public sealed class AppointmentsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ListAppointmentsResponse>(StatusCodes.Status200OK)]
    public async Task<ListAppointmentsResponse> ListAsync(
        [FromQuery] DateTime? startDateTime = null,
        [FromQuery] DateTime? endDateTime = null,
        [FromQuery] Guid? dentistId = null,
        [FromQuery] Guid? patientId = null,
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
            query = query.Where(appointment => appointment.Dentist.Id == new GuidEntityId<User>(dentistId.Value));
        }

        if (patientId is not null)
        {
            query = query.Where(appointment => appointment.Patient.Id == new GuidEntityId<Patient>(patientId.Value));
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
                        .Select(service => new ListAppointmentsResponseItemProvidedService
                        {
                            Id = service.Id.Value,
                            Name = service.Name
                        })
                        .ToList(),
                })
                .ToList()
        };
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetAppointmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<GetAppointmentResponse>, NotFound>> GetAsync(
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
                    Name = service.Name
                })
                .ToList()
        });
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Receptionist)}")]
    [ProducesResponseType<AddAppointmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<AddAppointmentResponse>, Conflict, NotFound>> AddAsync(
        [FromBody] AddAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var patient = await dbContext.Patients.GetByIdOrDefaultAsync(
            new GuidEntityId<Patient>(request.PatientId),
            cancellationToken);

        if (patient is null)
        {
            return TypedResults.NotFound();
        }

        var dentist = await dbContext.Dentists.GetByIdOrDefaultAsync(
            new GuidEntityId<User>(request.DentistId), 
            cancellationToken);

        if (dentist is null)
        {
            return TypedResults.NotFound();
        }

        var appointment = new Appointment(patient, dentist, request.StartTime, request.Duration);

        await dbContext.Appointments.AddAsync(appointment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new AddAppointmentResponse
        {
            Id = appointment.Id.Value
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Receptionist)}")]
    [ProducesResponseType<Ok>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<Results<Ok, NotFound, Conflict>> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var appointmentToUpdate = await dbContext.Appointments.GetByIdOrDefaultAsync(
            new GuidEntityId<Appointment>(id),
            cancellationToken);

        if (appointmentToUpdate is null)
        {
            return TypedResults.NotFound();
        }

        var patient = await dbContext.Patients.GetByIdOrDefaultAsync(
            new GuidEntityId<Patient>(request.PatientId),
            cancellationToken);

        if (patient is null)
        {
            return TypedResults.NotFound();
        }

        var dentist = await dbContext.Dentists.GetByIdOrDefaultAsync(
            new GuidEntityId<User>(request.DentistId), 
            cancellationToken);

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

        return TypedResults.Ok();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Receptionist)}")]
    [ProducesResponseType<Ok>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<Results<Ok, NotFound, Conflict>> CancelAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var appointmentToCancel = await dbContext.Appointments.GetByIdOrDefaultAsync(
            new GuidEntityId<Appointment>(id),
            cancellationToken);

        if (appointmentToCancel is null)
        {
            return TypedResults.NotFound();
        }

        if (appointmentToCancel.Status is not AppointmentStatus.Pending)
        {
            return TypedResults.Conflict();
        }

        appointmentToCancel.Cancel();

        dbContext.Appointments.Update(appointmentToCancel);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Dentist)}")]
    [ProducesResponseType<Ok>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<Results<Ok, NotFound, Conflict>> CompleteAsync(
        [FromRoute] Guid id,
        [FromBody] CompleteAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var appointmentToComplete = await dbContext.Appointments
            .Include(appointment => appointment.ProvidedServices)
            .GetByIdOrDefaultAsync(new GuidEntityId<Appointment>(id), cancellationToken);

        if (appointmentToComplete is null)
        {
            return TypedResults.NotFound();
        }

        if (appointmentToComplete.Status is not AppointmentStatus.Pending)
        {
            return TypedResults.Conflict();
        }

        var providedServices = await dbContext.Services
            .FilterByIds(request.ProvidedServiceIds.Select(id => new GuidEntityId<Service>(id)))
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

        return TypedResults.Ok();
    }

    [HttpPost("{id:guid}/pay")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Receptionist)}")]
    [ProducesResponseType<Ok>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<Results<Ok, NotFound, Conflict>> PayAsync(
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

        return TypedResults.Ok();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Receptionist)}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<Results<Ok, NotFound, Conflict>> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var appointmentToDelete = await dbContext.Appointments
            .AsNoTracking()
            .GetByIdOrDefaultAsync(new GuidEntityId<Appointment>(id), cancellationToken);

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

        return TypedResults.Ok();
    }
}
