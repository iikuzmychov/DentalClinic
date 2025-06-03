using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using DentalClinic.WebApi.Models.Patients;
using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain;

namespace DentalClinic.Tests.Integrational.Controllers;

public class PatientsControllerTests : TestBase
{
    public PatientsControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetPatients_ShouldReturnOkWithSeededData()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await Client.GetAsync("/api/patients");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var patientsResponse = JsonSerializer.Deserialize<ListPatientsResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(patientsResponse);
        Assert.True(patientsResponse.Items.Count >= 2);
        Assert.True(patientsResponse.TotalCount >= 2);
        
        // Check first patient exists
        var patient1 = patientsResponse.Items.FirstOrDefault(p => p.FirstName == "Іван");
        Assert.NotNull(patient1);
        Assert.Equal("Петренко", patient1.LastName);
        Assert.Equal("ivan.petrenko@example.com", patient1.Email);
        
        // Check second patient exists
        var patient2 = patientsResponse.Items.FirstOrDefault(p => p.FirstName == "Марія");
        Assert.NotNull(patient2);
        Assert.Equal("Іванова", patient2.LastName);
        Assert.Equal("maria.ivanova@example.com", patient2.Email);
    }

    [Fact]
    public async Task GetPatient_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await SeedDatabaseAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/patients/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePatient_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var createRequest = new AddPatientRequest
        {
            FirstName = "Іван",
            LastName = "Петренко",
            Surname = "Олександрович",
            Email = "ivan.petrenko@example.com",
            PhoneNumber = "+380123456789",
            Notes = "Тестові нотатки"
        };
        
        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/patients", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var patientResponse = JsonSerializer.Deserialize<AddPatientResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(patientResponse);
        Assert.NotEqual(Guid.Empty, patientResponse.Id);
    }

    [Fact]
    public async Task CreatePatient_WithEmptyFirstName_ShouldReturnInternalServerError()
    {
        // Arrange
        var createRequest = new AddPatientRequest
        {
            FirstName = "",
            LastName = "Петренко",
            Surname = "Олександрович",
            Email = "test@example.com",
            PhoneNumber = "+380123456789",
            Notes = "Test notes"
        };
        
        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/patients", content);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task CreatePatient_WithInvalidEmail_ShouldReturnInternalServerError()
    {
        // Arrange
        var createRequest = new AddPatientRequest
        {
            FirstName = "Іван",
            LastName = "Петренко", 
            Surname = "Олександрович",
            Email = "invalid-email",
            PhoneNumber = "+380123456789",
            Notes = "Test notes"
        };
        
        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/patients", content);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
} 