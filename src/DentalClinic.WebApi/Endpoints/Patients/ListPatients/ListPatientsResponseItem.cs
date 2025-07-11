namespace DentalClinic.WebApi.Endpoints.Patients.ListPatients;

public sealed record ListPatientsResponseItem
{
    public required Guid Id { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string? Surname { get; init; }
    public required string? Email { get; init; }
    public required string? PhoneNumber { get; init; }
}
