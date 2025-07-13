using DentalClinic.Domain.ValueObjects;
using DentalClinic.Infrastructure.Converters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DentalClinic.Infrastructure;

internal sealed class ApplicationValueConverterSelector : ValueConverterSelector
{
    public ApplicationValueConverterSelector(ValueConverterSelectorDependencies dependencies)
        : base(dependencies)
    {
    }

    public override IEnumerable<ValueConverterInfo> Select(Type modelClrType, Type? providerClrType = null)
    {
        foreach (var converter in base.Select(modelClrType, providerClrType))
        {
            yield return converter;
        }

        if (modelClrType.IsGenericType && modelClrType.GetGenericTypeDefinition() == typeof(GuidEntityId<>))
        {
            var entityType = modelClrType.GenericTypeArguments[0];
            var converterType = typeof(GuidEntityIdConverter<>).MakeGenericType(entityType);

            yield return new ValueConverterInfo(
                modelClrType,
                typeof(Guid),
                _ => (ValueConverter)Activator.CreateInstance(converterType)!);
        }
    }
}
