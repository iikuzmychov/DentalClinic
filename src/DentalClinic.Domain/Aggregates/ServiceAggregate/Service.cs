using DentalClinic.Domain.Interfaces;
using DentalClinic.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinic.Domain.Aggregates.ServiceAggregate;

public sealed class Service : IIdentifiable<GuidEntityId<Service>>
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public GuidEntityId<Service> Id { get; init; } = GuidEntityId<Service>.New();

    public required string Name
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value;
        }
    }
    public required Price Price
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }
}
