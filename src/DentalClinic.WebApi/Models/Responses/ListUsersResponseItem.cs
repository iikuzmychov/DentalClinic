using DentalClinic.Domain.Enums;

namespace DentalClinic.WebApi.Models.Responses;

public sealed record ListUsersResponseItem
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string? Surname { get; init; }
    public required Role Role { get; init; }
    public required string Email { get; init; }
    public required string? PhoneNumber { get; init; }
}
