namespace DentalClinic.WebApi.Models.Patients;

public sealed record AddPatientResponse
{
    public required Guid Id { get; init; }
}
