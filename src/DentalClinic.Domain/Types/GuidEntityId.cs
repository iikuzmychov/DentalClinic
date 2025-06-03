using DentalClinic.Domain.Interfaces;

namespace DentalClinic.Domain.Types;

public readonly record struct GuidEntityId<TEntity>(Guid Value) : IEntityId<TEntity, Guid>, IComparable<GuidEntityId<TEntity>>
{
    public static GuidEntityId<TEntity> New() => new(Guid.NewGuid());

    int IComparable<GuidEntityId<TEntity>>.CompareTo(GuidEntityId<TEntity> other)
        => Value.CompareTo(other.Value);
}
