namespace DentalClinic.WebApi.Models.Users;

public sealed record AddUserResponse
{
    public required Guid Id { get; init; }
}
