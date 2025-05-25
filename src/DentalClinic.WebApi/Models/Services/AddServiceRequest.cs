namespace DentalClinic.WebApi.Models.Services;

public sealed record AddServiceRequest
{
    public required string Name { get; init; }
    public required decimal Price { get; init; }
}
