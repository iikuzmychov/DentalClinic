using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Appointments.ListAppointments;

public sealed record ListAppointmentsResponseItem
{
    public required GuidEntityId<Appointment> Id { get; init; }
    public required ListAppointmentsResponseItemPatient Patient { get; init; }
    public required ListAppointmentsResponseItemDentist Dentist { get; init; }
    public required AppointmentStatus Status { get; init; }
    public required DateTime StartTime { get; init; }
    public required TimeSpan Duration { get; init; }
    public required List<ListAppointmentsResponseItemProvidedService> ProvidedServices { get; init; }
}
