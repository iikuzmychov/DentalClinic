using Microsoft.EntityFrameworkCore;
using DentalClinic.Domain.Interfaces;

namespace DentalClinic.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static Task<TEntity> GetByIdAsync<TEntity, TEntityId>(
        this IQueryable<TEntity> query,
        TEntityId id,
        CancellationToken cancellationToken)
        where TEntity : class, IIdentifiable<TEntityId>
        where TEntityId : IEntityId<TEntity>
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(id);

        return query.SingleAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    public static Task<TEntity?> GetByIdOrDefaultAsync<TEntity, TEntityId>(
        this IQueryable<TEntity> query,
        TEntityId id,
        CancellationToken cancellationToken)
        where TEntity : class, IIdentifiable<TEntityId>
        where TEntityId : IEntityId<TEntity>
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(id);

        return query.SingleOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);
    }

    public static IQueryable<TEntity> FilterByIds<TEntity, TEntityId>(
        this IQueryable<TEntity> query,
        IEnumerable<TEntityId> ids)
        where TEntity : class, IIdentifiable<TEntityId>
        where TEntityId : IEntityId<TEntity>
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(ids);

        return query.Where(entity => ids.Any(id => entity.Id.Equals(id)));
    }
}
