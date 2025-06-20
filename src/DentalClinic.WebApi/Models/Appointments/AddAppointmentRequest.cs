﻿namespace DentalClinic.WebApi.Models.Appointments;

public sealed record AddAppointmentRequest
{
    public required Guid PatientId { get; init; }
    public required Guid DentistId { get; init; }
    public required DateTime StartTime { get; init; }
    public required TimeSpan Duration { get; init; }
}
