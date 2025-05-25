using DentalClinic.Domain.Interfaces;
using DentalClinic.Domain.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinic.Domain.Aggregates.PatientAggregate;

public sealed class Patient : IIdentifiable<GuidEntityId<Patient>>
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public GuidEntityId<Patient> Id { get; set; } = GuidEntityId<Patient>.New();

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
    public string? Surname
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }
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
