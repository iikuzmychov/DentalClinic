namespace DentalClinic.WebApi.Endpoints.Users.UpdateUser;

public sealed record UpdateUserRequest
{
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public string? Surname { get; init; }
    public string? PhoneNumber { get; init; }
}
