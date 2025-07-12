using DentalClinic.Domain;

namespace DentalClinic.WebApi.Endpoints.Auth.Login;

public sealed record LoginRequest
{
    public required Email Email { get; init; }
    public required string Password { get; init; }
}
