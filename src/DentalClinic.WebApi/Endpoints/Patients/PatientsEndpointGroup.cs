namespace DentalClinic.WebApi.Endpoints.Patients;

internal class PatientsEndpointGroup : IEndpointGroup
{
    public RouteGroupBuilder Map(IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGroup("patients");
    }
}
