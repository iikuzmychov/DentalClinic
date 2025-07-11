namespace DentalClinic.WebApi.Endpoints.Appointments;

internal class AppointmentsEndpointGroup : IEndpointGroup
{
    public RouteGroupBuilder Map(IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGroup("appointments");
    }
}
