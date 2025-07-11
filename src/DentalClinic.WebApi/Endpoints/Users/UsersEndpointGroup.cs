namespace DentalClinic.WebApi.Endpoints.Users;

internal class UsersEndpointGroup : IEndpointGroup
{
    public RouteGroupBuilder Map(IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGroup("users")
            .WithTags("Users");
    }
}
