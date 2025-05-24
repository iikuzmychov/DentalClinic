namespace DentalClinic.WebApi.Models.Responses;

public sealed record LoginResponse
{
    public required string Token { get; init; }
}
