namespace DentalClinic.WebApi.Endpoints.Auth.Login;

public sealed record LoginResponse
{
    public required string Token { get; init; }
}
