namespace DentalClinic.WebApi.Models.Responses;

public sealed record AddUserResponse
{
    public required Guid Id { get; init; }
}
