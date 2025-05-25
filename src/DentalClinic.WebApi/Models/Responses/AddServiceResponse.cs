namespace DentalClinic.WebApi.Models.Responses;

public sealed record AddServiceResponse
{
    public required Guid Id { get; init; }
}
