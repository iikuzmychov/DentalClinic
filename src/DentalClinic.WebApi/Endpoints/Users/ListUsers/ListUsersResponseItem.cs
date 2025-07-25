﻿using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.WebApi.Endpoints.Users.ListUsers;

public sealed record ListUsersResponseItem
{
    public required GuidEntityId<User> Id { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public required string? Surname { get; init; }
    public required Role Role { get; init; }
    public required Email Email { get; init; }
    public required string? PhoneNumber { get; init; }
}
