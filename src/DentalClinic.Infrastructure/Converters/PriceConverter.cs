using DentalClinic.Domain.Aggregates.ServiceAggregate;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DentalClinic.Infrastructure.Converters;

internal sealed class PriceConverter : ValueConverter<Price, decimal>
{
    public PriceConverter()
        : base(
            price => price.Value,
            value => new Price(value))
    {
    }
}
