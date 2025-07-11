namespace DentalClinic.WebApi.Endpoints.Appointments.ListAppointments;

public sealed record ListAppointmentsResponseItemProvidedService
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}
