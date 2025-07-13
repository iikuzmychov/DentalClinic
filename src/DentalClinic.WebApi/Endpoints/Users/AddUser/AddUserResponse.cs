using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Users.AddUser;

public sealed record AddUserResponse
{
    public required GuidEntityId<User> Id { get; init; }
}
