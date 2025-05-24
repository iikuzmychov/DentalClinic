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

        return query.SingleOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);
    }
}
