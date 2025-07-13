using DentalClinic.Domain;

namespace DentalClinic.WebApi.Endpoints.Patients.UpdatePatient;

public sealed record UpdatePatientRequest
{
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public string? Surname { get; init; }
    public Email? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Notes { get; init; }
}
