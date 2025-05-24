using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

namespace DentalClinic.Infrastructure.Extensions;

public static class ModelConfigurationBuilderExtensions
{
    public static ModelConfigurationBuilder ApplyConversionsFromAssembly(
        this ModelConfigurationBuilder configurationBuilder,
        Assembly assembly)
    {
        var valueConverterTypes = assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && !type.IsGenericType)
            .Where(type => type
                .GetInheritedTypes()
                .Any(inheritedType =>
                    inheritedType.IsGenericType &&
                    inheritedType.GetGenericTypeDefinition() == typeof(ValueConverter<,>)));

        foreach (var valueConverterType in valueConverterTypes)
        {
            var propertyType = valueConverterType.BaseType!.GetGenericArguments().First();
            configurationBuilder.Properties(propertyType).HaveConversion(valueConverterType);
        }

        return configurationBuilder;
    }
}
