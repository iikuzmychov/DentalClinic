using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Appointments.AddAppointment;

public sealed record AddAppointmentResponse
{
    public required GuidEntityId<Appointment> Id { get; init; }
}
