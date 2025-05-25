namespace DentalClinic.WebApi.Models.Appointments;

public sealed record AddAppointmentResponse
{
    public required Guid Id { get; init; }
}
