namespace DentalClinic.WebApi.Endpoints.Appointments.GetAppointment;

public sealed record GetAppointmentResponseItemPatient
{
    public required Guid Id { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string? Surname { get; init; }
}

