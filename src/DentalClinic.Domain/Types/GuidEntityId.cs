using DentalClinic.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace DentalClinic.Domain.Types;

public readonly record struct GuidEntityId<TEntity>(Guid Value) : IEntityId<TEntity, Guid>, IComparable<GuidEntityId<TEntity>>, IParsable<GuidEntityId<TEntity>>
{
    public static GuidEntityId<TEntity> New() => new(Guid.NewGuid());

    public static GuidEntityId<TEntity> Parse(string s, IFormatProvider? provider)
        => new(Guid.Parse(s, provider));

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out GuidEntityId<TEntity> result)
    {
        if (!Guid.TryParse(s, provider, out var guidValue))
        {
            result = default;
            return false;
        }

        result = new(guidValue);
        return true;
    }

    int IComparable<GuidEntityId<TEntity>>.CompareTo(GuidEntityId<TEntity> other)
        => Value.CompareTo(other.Value);
}
