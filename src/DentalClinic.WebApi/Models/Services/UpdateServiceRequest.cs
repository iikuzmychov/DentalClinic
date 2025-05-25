namespace DentalClinic.WebApi.Models.Services;

public sealed record UpdateServiceRequest
{
    public required string Name { get; init; }
    public required decimal Price { get; init; }
}
