namespace DentalClinic.WebApi.Endpoints;

public interface IEndpoint<T>
    where T : IEndpointGroup
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group);
}
