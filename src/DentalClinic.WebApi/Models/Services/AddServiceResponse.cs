namespace DentalClinic.WebApi.Models.Services;

public sealed record AddServiceResponse
{
    public required Guid Id { get; init; }
}
