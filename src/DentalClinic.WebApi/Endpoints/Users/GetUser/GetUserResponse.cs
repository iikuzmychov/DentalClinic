using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.Types;

namespace DentalClinic.WebApi.Endpoints.Users.GetUser;

public sealed record GetUserResponse
{
    public required GuidEntityId<User> Id { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string? Surname { get; init; }
    public required Role Role { get; init; }
    public required string Email { get; init; }
    public required string? PhoneNumber { get; init; }
}
