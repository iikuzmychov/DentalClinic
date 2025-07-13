using DentalClinic.Domain;
using DentalClinic.Domain.ValueObjects;
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
            options.AddSchemaTransformer(MapEmailToSchemaAsync);
            options.AddSchemaTransformer(MapSecurePasswordToSchemaAsync);

            options.CreateSchemaReferenceId = CreateSchemaReferenceId;
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
            MakeUuid(schema);
            return Task.CompletedTask;
        }

        if (schema.Type == "array" &&
            schema.Items is null &&
            context.JsonTypeInfo.ElementType?.IsGenericType == true &&
            context.JsonTypeInfo.ElementType.GetGenericTypeDefinition() == typeof(GuidEntityId<>))
        {
            schema.Items = new OpenApiSchema();
            MakeUuid(schema.Items);
        }

        return Task.CompletedTask;

        static void MakeUuid(OpenApiSchema target)
        {
            target.Type = "string";
            target.Format = "uuid";
            target.Reference = null;
            target.Properties?.Clear();
            target.Required?.Clear();
        }
    }

    private static Task MapEmailToSchemaAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type == typeof(Email))
        {
            schema.Type = "string";
            schema.MinLength = Email.MinLength;
            schema.MaxLength = Email.MaxLength;
            schema.Pattern = Email.Regex().ToString();
            schema.Reference = null;
            schema.Properties?.Clear();
            schema.Required?.Clear();
        }

        return Task.CompletedTask;
    }

    private static Task MapSecurePasswordToSchemaAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type == typeof(SecurePassword))
        {
            schema.Type = "string";
            schema.MinLength = SecurePassword.MinLength;
            schema.MaxLength = SecurePassword.MaxLength;
            schema.Pattern = SecurePassword.Regex().ToString();
            schema.Reference = null;
            schema.Properties?.Clear();
            schema.Required?.Clear();
        }

        return Task.CompletedTask;
    }

    private static string? CreateSchemaReferenceId(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type.IsGenericType &&
            typeInfo.Type.GetGenericTypeDefinition() == typeof(GuidEntityId<>))
        {
            return null;
        }

        if (typeInfo.Type == typeof(Email))
        {
            return null;
        }

        if (typeInfo.Type == typeof(SecurePassword))
        {
            return null;
        }

        return OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);
    }
}
