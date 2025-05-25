using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace DentalClinic.WebApi.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddApplicationOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer(AddBearerSchemeAsync);
            options.AddOperationTransformer(ConfigureEndpointAuthorizationAsync);
        });

        return services;
    }

    private static Task AddBearerSchemeAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var bearerScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme,
            }
        };

        document.Components ??= new();
        document.Components.SecuritySchemes.Add(JwtBearerDefaults.AuthenticationScheme, bearerScheme);

        return Task.CompletedTask;
    }

    private static Task ConfigureEndpointAuthorizationAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            return Task.CompletedTask;
        }

        var methodAllowAnonymousAttribute = controllerActionDescriptor
            .MethodInfo
            .GetCustomAttribute<AllowAnonymousAttribute>();

        var controllerAllowAnonymousAttribute = controllerActionDescriptor
            .ControllerTypeInfo
            .GetCustomAttribute<AllowAnonymousAttribute>();

        var isAuthorizationRequired =
            methodAllowAnonymousAttribute is null &&
            controllerAllowAnonymousAttribute is null;

        if (!isAuthorizationRequired)
        {
            return Task.CompletedTask;
        }

        var bearerScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            },
            UnresolvedReference = true
        };

        var securityRequirement = new OpenApiSecurityRequirement
        {
            [bearerScheme] = []
        };

        operation.Security.Add(securityRequirement);

        return Task.CompletedTask;
    }
}
