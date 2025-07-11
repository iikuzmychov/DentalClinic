using System.Text.Json;
using System.Text.Json.Serialization;

namespace DentalClinic.Domain.JsonConverters;

public sealed class JsonEmailConverter : JsonConverter<Email>
{
    public override Email? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => new Email(reader.GetString()!),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}. Expected a string or null.")
        };

    public override void Write(Utf8JsonWriter writer, Email value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}
