using System.Text.Json;
using System.Text.Json.Serialization;

namespace DentalClinic.Domain.JsonConverters;

public sealed class JsonSecurePasswordConverter : JsonConverter<SecurePassword>
{
    public override SecurePassword? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => SecurePassword.Parse(reader.GetString()!),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}. Expected a string or null.")
        };

    public override void Write(Utf8JsonWriter writer, SecurePassword value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}
