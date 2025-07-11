namespace DentalClinic.WebApi.Endpoints.Services.AddService;

public sealed record AddServiceRequest
{
    public required string Name { get; init; }
    public required decimal Price { get; init; }
}
