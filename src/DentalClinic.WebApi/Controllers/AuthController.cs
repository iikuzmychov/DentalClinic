using DentalClinic.Infrastructure;
using DentalClinic.WebApi.Models.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DentalClinic.WebApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HttpValidationProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult>> LoginAsync(
        [FromBody] LoginRequest request,
        [FromServices] IOptionsMonitor<JwtBearerOptions> jwtBearerOptionsMonitor)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == (object)request.Email);

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
            new(JwtRegisteredClaimNames.Iss, jwtBearerOptions.TokenValidationParameters.ValidIssuer),
            new(Constants.JwtRoleClaimName, user.Role.ToString())
        };

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
