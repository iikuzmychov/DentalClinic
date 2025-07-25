﻿using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Patients.GetPatient;

public sealed record GetPatientResponse
{
    public required GuidEntityId<Patient> Id { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string? Surname { get; init; }
    public required string? Email { get; init; }
    public required string? PhoneNumber { get; init; }
    public required string? Notes { get; init; }
}
