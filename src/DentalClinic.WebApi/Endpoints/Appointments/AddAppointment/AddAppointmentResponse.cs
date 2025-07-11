namespace DentalClinic.WebApi.Endpoints.Appointments.AddAppointment;

public sealed record AddAppointmentResponse
{
    public required Guid Id { get; init; }
}
