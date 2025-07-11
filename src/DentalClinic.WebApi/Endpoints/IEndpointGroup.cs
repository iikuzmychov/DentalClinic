namespace DentalClinic.WebApi.Endpoints;

public interface IEndpointGroup
{
    public RouteGroupBuilder Map(IEndpointRouteBuilder endpoints);
}
