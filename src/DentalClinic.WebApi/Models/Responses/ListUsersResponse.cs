namespace DentalClinic.WebApi.Models.Responses;

public sealed record ListUsersResponse
{
    public required List<ListUsersResponseItem> Items { get; init; }
    public required int TotalPagesCount { get; init; }
    public required int TotalCount { get; init; }
}
