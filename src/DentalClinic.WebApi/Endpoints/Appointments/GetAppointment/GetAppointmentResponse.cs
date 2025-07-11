using DentalClinic.Domain.Enums;

namespace DentalClinic.WebApi.Endpoints.Appointments.GetAppointment;

public sealed record GetAppointmentResponse
{
    public required Guid Id { get; init; }
    public required GetAppointmentResponseItemPatient Patient { get; init; }
    public required GetAppointmentResponseItemDentist Dentist { get; init; }
    public required AppointmentStatus Status { get; init; }
    public required DateTime StartTime { get; init; }
    public required TimeSpan Duration { get; init; }
    public required List<GetAppointmentResponseItemProvidedService> ProvidedServices { get; init; }
}
