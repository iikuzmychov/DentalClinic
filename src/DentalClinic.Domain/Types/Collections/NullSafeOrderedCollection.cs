namespace DentalClinic.Domain.Types.Collections;

public class NullSafeOrderedCollection<T> : NullSafeCollection<OrderedItem<T>>
    where T : notnull
{
    public NullSafeOrderedCollection()
    {
    }

    public NullSafeOrderedCollection(IList<OrderedItem<T>> collection) : base(collection)
    {
    }

    public NullSafeOrderedCollection(IEnumerable<OrderedItem<T>> enumerable) : base(enumerable.ToList())
    {
    }
}
