using DentalClinic.Domain.Types;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DentalClinic.Domain.JsonConverters;

public sealed class JsonGuidEntityIdConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(GuidEntityId<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var entityType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(JsonGuidEntityIdConverter<>).MakeGenericType(entityType);

        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}
