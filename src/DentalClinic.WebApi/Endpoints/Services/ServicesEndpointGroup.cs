namespace DentalClinic.WebApi.Endpoints.Services;

internal class ServicesEndpointGroup : IEndpointGroup
{
    public RouteGroupBuilder Map(IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGroup("services")
            .WithTags("Services");
    }
}
