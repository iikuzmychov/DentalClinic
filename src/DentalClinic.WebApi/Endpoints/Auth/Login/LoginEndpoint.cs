using DentalClinic.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DentalClinic.WebApi.Endpoints.Auth.Login;

internal sealed class LoginEndpoint : IEndpoint<AuthEndpointGroup>
{
    public RouteHandlerBuilder Map(RouteGroupBuilder group)
    {
        return group
            .MapPost("login", HandleAsync)
            .AllowAnonymous();
    }

    private static async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> HandleAsync(
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] IOptionsMonitor<JwtBearerOptions> jwtBearerOptionsMonitor,
        [FromBody] LoginRequest request)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == request.Email);

        if (user is null || !user.HashedPassword.IsMatch(request.Password))
        {
            return TypedResults.Unauthorized();
        }

        var jwtBearerOptions = jwtBearerOptionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);

        var signingCredentials = new SigningCredentials(
            jwtBearerOptions.TokenValidationParameters.IssuerSigningKeys.Single(),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Iss, jwtBearerOptions.TokenValidationParameters.ValidIssuer),
            new(Constants.JwtRoleClaimName, user.Role.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Surname))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.MiddleName, user.Surname));
        }

        foreach (var validAudience in jwtBearerOptions.TokenValidationParameters.ValidAudiences)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, validAudience));
        }

        var accessToken = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(Constants.DefaultTokenExpirationTimeout),
            signingCredentials: signingCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(accessToken);

        return TypedResults.Ok(new LoginResponse
        {
            Token = token
        });
    }
}
