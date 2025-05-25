namespace DentalClinic.WebApi.Models.Appointments;

public sealed record GetAppointmentResponseItemProvidedService
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}
