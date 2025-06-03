using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DentalClinic.Tests.Integrational;

public static class TestJwtHelper
{
    private const string TestSecretKey = "ThisIsAVerySecureTestKeyForJWT12345"; // Минимум 32 символа
    private const string TestIssuer = "dotnet-user-jwts";
    private const string TestAudience = "https://localhost:7227";

    public static string GenerateTestToken(
        string userId = "test-user-id",
        string email = "test@example.com",
        string firstName = "Test",
        string lastName = "User",
        string role = "Admin",
        string? surname = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.GivenName, firstName),
            new(JwtRegisteredClaimNames.FamilyName, lastName),
            new(JwtRegisteredClaimNames.Iss, TestIssuer),
            new(JwtRegisteredClaimNames.Aud, TestAudience),
            new("role", role)
        };

        if (!string.IsNullOrWhiteSpace(surname))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.MiddleName, surname));
        }

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static SymmetricSecurityKey GetTestSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
    }
} 