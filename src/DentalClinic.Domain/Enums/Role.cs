using System.Text.Json.Serialization;

namespace DentalClinic.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<Role>))]
public enum Role
{
    Admin,
    Dentist,
    Receptionist
}
