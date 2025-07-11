namespace DentalClinic.WebApi.Endpoints.Users.UpdateUser;

public sealed record UpdateUserRequest
{
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string? Surname { get; init; }
    public required string? PhoneNumber { get; init; }
}
