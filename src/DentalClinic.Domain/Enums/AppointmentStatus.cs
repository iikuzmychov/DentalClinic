using System.Text.Json.Serialization;

namespace DentalClinic.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<AppointmentStatus>))]
public enum AppointmentStatus
{
    Scheduled,
    Cancelled,
    Completed,
    Paid
}
