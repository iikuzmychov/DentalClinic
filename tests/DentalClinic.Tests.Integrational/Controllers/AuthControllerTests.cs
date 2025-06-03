using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using DentalClinic.WebApi.Models.Auth;
using System.Net;

namespace DentalClinic.Tests.Integrational.Controllers;

public class AuthControllerTests : TestBase
{
    public AuthControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var loginRequest = new LoginRequest
        {
            Email = "invalid@example.com",
            Password = "InvalidPassword123!"
        };
        
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Используем неавторизованный клиент для логина
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "",
            Password = "ValidPassword123!"
        };
        
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Используем неавторизованный клиент для логина
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMalformedJson_ShouldReturnBadRequest()
    {
        // Arrange
        var malformedJson = "{ invalid json }";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        // Используем неавторизованный клиент для логина
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithoutContentType_ShouldReturnUnsupportedMediaType()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };
        
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "text/plain"); // Неправильный Content-Type

        // Используем неавторизованный клиент для логина
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }
} 