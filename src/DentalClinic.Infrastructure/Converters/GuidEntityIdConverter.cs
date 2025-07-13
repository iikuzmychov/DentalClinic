using DentalClinic.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DentalClinic.Infrastructure.Converters;

internal sealed class GuidEntityIdConverter<TEntity> : ValueConverter<GuidEntityId<TEntity>, Guid>
{
    public GuidEntityIdConverter()
        : base(
            entityId => entityId.Value,
            value => new GuidEntityId<TEntity>(value))
    {
    }
}
