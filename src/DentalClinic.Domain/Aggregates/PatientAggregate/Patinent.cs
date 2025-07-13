using DentalClinic.Domain.Interfaces;
using DentalClinic.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinic.Domain.Aggregates.PatientAggregate;

public sealed class Patient : IIdentifiable<GuidEntityId<Patient>>
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public GuidEntityId<Patient> Id { get; init; } = GuidEntityId<Patient>.New();

    public required string FirstName
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value;
        }
    }
    public required string LastName
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value;
        }
    }
    public string? Surname { get; set; }
    public Email? Email { get; set; }
    public string? PhoneNumber
    {
        get;
        set
        {
            if (value is null)
            {
                field = null;
                return;
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value;
        }
    }
    public string? Notes { get; set; }
}
