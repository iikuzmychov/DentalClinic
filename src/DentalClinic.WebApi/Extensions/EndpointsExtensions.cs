using DentalClinic.WebApi.Endpoints;
using System.Reflection;

namespace DentalClinic.WebApi.Extensions;

public static class EndpointsExtensions
{
    public static IServiceCollection AddApplicationEndpoints(this IServiceCollection services)
    {
        var assembly = typeof(EndpointsExtensions).Assembly;

        var endpointGroupTypes = assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                !type.IsInterface &&
                typeof(IEndpointGroup).IsAssignableFrom(type));

        foreach (var endpointGroupType in endpointGroupTypes)
        {
            services.AddSingleton(typeof(IEndpointGroup), endpointGroupType);
        }

        var endpointTypes = assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                !type.IsInterface &&
                type.GetInterfaces().Any(implementedInterface =>
                    implementedInterface.IsGenericType &&
                    implementedInterface.GetGenericTypeDefinition() == typeof(IEndpoint<>)));

        foreach (var endpointType in endpointTypes)
        {
            var endpointInterface = endpointType
                .GetInterfaces()
                .Single(implementedInterface =>
                    implementedInterface.IsGenericType &&
                    implementedInterface.GetGenericTypeDefinition() == typeof(IEndpoint<>));

            services.AddSingleton(endpointInterface, endpointType);
        }

        return services;
    }

    public static WebApplication MapApplicationEndpoints(this WebApplication webApplication)
    {
        var apiGroup = webApplication.MapGroup("api");
        var endpointsGroups = webApplication.Services.GetServices<IEndpointGroup>();
        
        foreach (var endpointGroup in endpointsGroups)
        {
            var group = endpointGroup
                .Map(apiGroup)
                .WithTags(endpointGroup.GetType().Name.Replace(nameof(IEndpointGroup)[1..], string.Empty))
                .WithOpenApi();

            var endpointType = typeof(IEndpoint<>).MakeGenericType(endpointGroup.GetType());
            var endpoints = webApplication.Services.GetServices(endpointType);

            var mapEndpointMethod = typeof(EndpointsExtensions)
                .GetMethod(nameof(MapEndpoint), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(endpointGroup.GetType());

            foreach (var endpoint in endpoints)
            {
                mapEndpointMethod.Invoke(null, [endpoint, group]);
            }
        }

        return webApplication;
    }

    private static void MapEndpoint<T>(IEndpoint<T> endpoint, RouteGroupBuilder group) where T : IEndpointGroup
        => endpoint.Map(group);
}
