
namespace DentalClinic.WebApi.Endpoints.Auth;

internal class AuthEndpointGroup : IEndpointGroup
{
    public RouteGroupBuilder Map(IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGroup("auth");
    }
}
