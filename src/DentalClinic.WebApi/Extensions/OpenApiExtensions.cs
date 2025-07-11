using DentalClinic.Domain.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization.Metadata;

namespace DentalClinic.WebApi.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddApplicationOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer(SortPathsAndOperationsAsync);
            options.AddDocumentTransformer(AddBearerSchemeAsync);
            options.AddOperationTransformer(ConfigureEndpointSecurityAsync);
            options.AddSchemaTransformer(MapGuidEntityIdToUuidAsync);
            options.CreateSchemaReferenceId = CreateDefaultSchemaReferenceId;
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

    private static Task ConfigureEndpointSecurityAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var isAuthorizationRequired = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<AllowAnonymousAttribute>()
            .Any();

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

    private static Task SortPathsAndOperationsAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var orderedPaths = document.Paths
            .OrderBy(path => path.Key.Length)
            .ThenBy(path => path.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var newPaths = new OpenApiPaths();

        foreach (var (path, pathItem) in orderedPaths)
        {
            var orderedOperations = pathItem.Operations
                .OrderBy(operation => operation.Key switch
                {
                    OperationType.Get => 0,
                    OperationType.Post => 1,
                    OperationType.Put => 2,
                    OperationType.Delete => 3,
                    _ => 4
                })
                .ToList();

            var newItem = new OpenApiPathItem
            {
                Summary = pathItem.Summary,
                Description = pathItem.Description,
                Servers = pathItem.Servers,
                Extensions = pathItem.Extensions,
                Parameters = pathItem.Parameters
            };

            foreach (var (method, operation) in orderedOperations)
            {
                newItem.Operations.Add(method, operation);
            }

            newPaths.Add(path, newItem);
        }

        document.Paths = newPaths;

        return Task.CompletedTask;
    }

    private static Task MapGuidEntityIdToUuidAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type.IsGenericType &&
            context.JsonTypeInfo.Type.GetGenericTypeDefinition() == typeof(GuidEntityId<>))
        {
            schema.Type = "string";
            schema.Format = "uuid";
            schema.Reference = null;
            schema.Properties?.Clear();
        }

        return Task.CompletedTask;
    }

    private static string? CreateDefaultSchemaReferenceId(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type.IsGenericType &&
            typeInfo.Type.GetGenericTypeDefinition() == typeof(GuidEntityId<>))
        {
            return null;
        }

        return OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);
    }
}
