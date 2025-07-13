using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Patients.AddPatient;

public sealed record AddPatientResponse
{
    public required GuidEntityId<Patient> Id { get; init; }
}
