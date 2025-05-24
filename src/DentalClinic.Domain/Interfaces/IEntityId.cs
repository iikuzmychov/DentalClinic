namespace DentalClinic.Domain.Interfaces;

public interface IEntityId<TEntity>
{
}

public interface IEntityId<TEntity, TValue> : IEntityId<TEntity>
{
    public TValue Value { get; }
}
