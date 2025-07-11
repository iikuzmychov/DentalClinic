namespace DentalClinic.WebApi.Endpoints.Users.AddUser;

public sealed record AddUserResponse
{
    public required Guid Id { get; init; }
}
