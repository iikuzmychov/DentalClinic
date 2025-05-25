namespace DentalClinic.WebApi.Models.Requests;

public sealed record AddServiceRequest
{
    public required string Name { get; init; }
    public required decimal Price { get; init; }
}
