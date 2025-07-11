namespace DentalClinic.WebApi.Endpoints.Patients.UpdatePatient;

public sealed record UpdatePatientRequest
{
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string? Surname { get; init; }
    public required string? Email { get; init; }
    public required string? PhoneNumber { get; init; }
    public required string? Notes { get; init; }
}
