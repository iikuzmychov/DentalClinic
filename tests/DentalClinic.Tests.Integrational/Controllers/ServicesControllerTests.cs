using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using DentalClinic.WebApi.Endpoints.Services.AddService;
using DentalClinic.WebApi.Endpoints.Services.ListServices;

namespace DentalClinic.Tests.Integrational.Controllers;

public class ServicesControllerTests : TestBase
{
    public ServicesControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetServices_ShouldReturnOkWithSeededData()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await Client.GetAsync("/api/services");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var servicesResponse = JsonSerializer.Deserialize<ListServicesResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(servicesResponse);
        Assert.True(servicesResponse.Items.Count >= 2);
        Assert.True(servicesResponse.TotalCount >= 2);
        
        // Check first service exists
        var service1 = servicesResponse.Items.FirstOrDefault(s => s.Name == "Чистка зубів");
        Assert.NotNull(service1);
        Assert.Equal(500.00m, service1.Price);
        
        // Check second service exists
        var service2 = servicesResponse.Items.FirstOrDefault(s => s.Name == "Лікування карієсу");
        Assert.NotNull(service2);
        Assert.Equal(800.00m, service2.Price);
    }

    [Fact]
    public async Task GetService_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await SeedDatabaseAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/services/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateService_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var createRequest = new AddServiceRequest
        {
            Name = "Унікальна послуга " + Guid.NewGuid().ToString("N")[..8],
            Price = 500.00m
        };
        
        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/services", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var serviceResponse = JsonSerializer.Deserialize<AddServiceResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(serviceResponse);
        Assert.NotEqual(Guid.Empty, serviceResponse.Id);
    }

    [Fact]
    public async Task CreateService_WithEmptyName_ShouldReturnInternalServerError()
    {
        // Arrange
        var createRequest = new AddServiceRequest
        {
            Name = "",
            Price = 500.00m
        };
        
        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/services", content);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task CreateService_WithNegativePrice_ShouldReturnInternalServerError()
    {
        // Arrange
        var createRequest = new AddServiceRequest
        {
            Name = "Унікальна послуга " + Guid.NewGuid().ToString("N")[..8],
            Price = -100.00m
        };
        
        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/services", content);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task CreateService_WithZeroPrice_ShouldReturnInternalServerError()
    {
        // Arrange
        var createRequest = new AddServiceRequest
        {
            Name = "Унікальна послуга " + Guid.NewGuid().ToString("N")[..8],
            Price = 0m
        };
        
        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/services", content);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
} 
