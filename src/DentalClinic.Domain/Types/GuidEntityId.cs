using DentalClinic.Domain.Interfaces;

namespace DentalClinic.Domain.Types;

public readonly record struct GuidEntityId<TEntity>(Guid Value) : IEntityId<TEntity, Guid>
{
    public static GuidEntityId<TEntity> New() => new(Guid.NewGuid());
}
