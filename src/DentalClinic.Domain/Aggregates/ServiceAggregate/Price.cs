namespace DentalClinic.Domain.Aggregates.ServiceAggregate;

public sealed record Price
{
    public decimal Value { get; }

    public Price(decimal value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        Value = value;
    }
}
