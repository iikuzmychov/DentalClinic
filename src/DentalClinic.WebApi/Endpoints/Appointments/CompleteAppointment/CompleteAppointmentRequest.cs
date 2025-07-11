namespace DentalClinic.WebApi.Endpoints.Appointments.CompleteAppointment;

public sealed record CompleteAppointmentRequest
{
    public required TimeSpan Duration { get; init; }
    public required List<Guid> ProvidedServiceIds { get; init; }
}
