using DentalClinic.WebApi.Models.Services;

namespace DentalClinic.WebApi.Models.Responses;

public sealed record ListServicesResponse
{
    public required List<ListServicesResponseItem> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int TotalPagesCount { get; init; }
}
