using DentalClinic.Domain.Enums;

namespace DentalClinic.WebApi.Models.Users;

public sealed record AddUserRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string? Surname { get; init; }
    public required Role Role { get; init; }
    public required string Email { get; init; }
    public required string? PhoneNumber { get; init; }
    public required string Password { get; init; }
}
