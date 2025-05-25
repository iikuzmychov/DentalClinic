using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Interfaces;
using DentalClinic.Domain.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinic.Domain.Aggregates.UserAggregate;

public sealed class User : IIdentifiable<GuidEntityId<User>>
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public GuidEntityId<User> Id { get; set; } = GuidEntityId<User>.New();

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
    public required Role Role
    {
        get;
        init
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            field = value;
        }
    }
    public required Email Email
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }
    public required HashedPassword HashedPassword
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }
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
}
