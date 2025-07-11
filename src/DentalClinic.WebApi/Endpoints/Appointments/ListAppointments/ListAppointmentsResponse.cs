namespace DentalClinic.WebApi.Endpoints.Appointments.ListAppointments;

public sealed record ListAppointmentsResponse
{
    public required List<ListAppointmentsResponseItem> Items { get; init; }
}
