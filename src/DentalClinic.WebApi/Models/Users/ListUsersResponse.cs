namespace DentalClinic.WebApi.Models.Users;

public sealed record ListUsersResponse
{
    public required List<ListUsersResponseItem> Items { get; init; }
    public required int TotalPagesCount { get; init; }
    public required int TotalCount { get; init; }
}
