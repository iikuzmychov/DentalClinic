namespace DentalClinic.Domain.Aggregates.ServiceAggregate;

public sealed partial record Price
{
    public decimal Value { get; }

    public Price(decimal value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        Value = value;
    }
}
