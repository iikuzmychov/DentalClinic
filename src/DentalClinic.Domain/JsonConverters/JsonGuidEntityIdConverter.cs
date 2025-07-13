using DentalClinic.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DentalClinic.Domain.JsonConverters;

public sealed class JsonGuidEntityIdConverter<TEntity> : JsonConverter<GuidEntityId<TEntity>>
{
    public override GuidEntityId<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetGuid());

    public override void Write(Utf8JsonWriter writer, GuidEntityId<TEntity> value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}
