using DentalClinic.Domain.JsonConverters;
using DentalClinic.Infrastructure;
using DentalClinic.WebApi;
using DentalClinic.WebApi.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString(Constants.DefaultConnectionStringName);
builder.Services.AddApplicationInfrastructure(connectionString);

builder.Services
    .AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
        options.TokenValidationParameters.RoleClaimType = Constants.JwtRoleClaimName;
    });

builder.Services
    .AddAuthorizationBuilder()
        .SetFallbackPolicy(new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build());

builder.Services.AddCors();

builder.Services.AddApplicationEndpoints();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationOpenApi();

builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonGuidEntityIdConverterFactory());
    options.SerializerOptions.Converters.Add(new JsonEmailConverter());
});

var application = builder.Build();

application.UseHttpsRedirection();

if (application.Environment.IsDevelopment())
{
    application.UseCors(builder => builder
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin());
}
else
{
    application.UseExceptionHandler();
}

application.UseAuthentication();
application.UseAuthorization();

application.MapApplicationEndpoints();

if (application.Environment.IsDevelopment() || EnvironmentHelper.IsOpenApiGeneration())
{
    application
        .MapOpenApi()
        .AllowAnonymous();

    application
        .MapScalarApiReference(options =>
        {
            options
                .AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme)
                .WithPersistentAuthentication(true)
                .WithDownloadButton(false);
        })
        .AllowAnonymous();
}

if (application.Environment.IsDevelopment())
{
    await using var scope = application.Services.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

application.Run();

public partial class Program { }
