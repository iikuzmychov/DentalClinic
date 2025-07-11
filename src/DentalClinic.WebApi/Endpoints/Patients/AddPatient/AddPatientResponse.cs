namespace DentalClinic.WebApi.Endpoints.Patients.AddPatient;

public sealed record AddPatientResponse
{
    public required Guid Id { get; init; }
}
