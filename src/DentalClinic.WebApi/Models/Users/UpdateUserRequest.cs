namespace DentalClinic.WebApi.Models.Requests;

public sealed record UpdateUserRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string? Surname { get; init; }
    public required string? PhoneNumber { get; init; }
}
