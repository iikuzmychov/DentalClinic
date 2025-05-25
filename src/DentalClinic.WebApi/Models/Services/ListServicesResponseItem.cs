namespace DentalClinic.WebApi.Models.Services;

public sealed record ListServicesResponseItem
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
}
