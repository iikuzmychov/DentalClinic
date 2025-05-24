namespace DentalClinic.Domain.Types;

public sealed class OrderedItem<T>
    where T : notnull
{
    public int Order { get; private set; }
    public T Item { get; private set; }

    [Obsolete("see <URL: https://github.com/dotnet/efcore/issues/12078 >")]
    private OrderedItem()
    {
        Item = default!;
    }

    public OrderedItem(int order, T item)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(order);
        ArgumentNullException.ThrowIfNull(item);

        Order = order;
        Item = item;
    }
}
