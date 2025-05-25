namespace DentalClinic.WebApi.Models.Appointments;

public sealed record CompleteAppointmentRequest
{
    public required TimeSpan Duration { get; init; }
    public required List<Guid> ProvidedServiceIds { get; init; }
}
