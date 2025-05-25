namespace DentalClinic.WebApi.Models.Auth;

public sealed record LoginResponse
{
    public required string Token { get; init; }
}
