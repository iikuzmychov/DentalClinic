namespace DentalClinic.WebApi.Models.Appointments;

public sealed record ListAppointmentsResponse
{
    public required List<ListAppointmentsResponseItem> Items { get; init; }
}
