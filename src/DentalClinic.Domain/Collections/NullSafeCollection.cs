using System.Collections.ObjectModel;

namespace DentalClinic.Domain.Collections;

public class NullSafeCollection<T> : Collection<T>
    where T : notnull
{
    public NullSafeCollection()
    {
    }

    public NullSafeCollection(IList<T> collection) : base(collection)
    {
    }

    public NullSafeCollection(IEnumerable<T> enumerable) : base(enumerable.ToList())
    {
    }

    public void AddRange(IList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Any(item => item == null))
        {
            throw new ArgumentNullException(nameof(items), "The collection contains null items.");
        }

        foreach (var item in items)
        {
            InsertItem(index: Count, item);
        }
    }

    protected override void InsertItem(int index, T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        base.SetItem(index, item);
    }
}
