namespace DentalClinic.WebApi.Endpoints.Patients.ListPatients;

public sealed record ListPatientsResponse
{
    public required List<ListPatientsResponseItem> Items { get; init; }
    public required int TotalPagesCount { get; init; }
    public required int TotalCount { get; init; }
}
