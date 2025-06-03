using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using DentalClinic.WebApi.Models.Appointments;
using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain;

namespace DentalClinic.Tests.Integrational.Controllers;

public class AppointmentsControllerTests : TestBase
{
    public AppointmentsControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetAppointments_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        await SeedDatabaseAsync();

        // Act
        var response = await Client.GetAsync("/api/appointments");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var appointmentsResponse = JsonSerializer.Deserialize<ListAppointmentsResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(appointmentsResponse);
        Assert.Empty(appointmentsResponse.Items);
    }

    [Fact]
    public async Task GetAppointment_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await SeedDatabaseAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/appointments/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAppointmentsByPatient_WithInvalidPatientId_ShouldReturnNotFound()
    {
        // Arrange
        await SeedDatabaseAsync();
        var invalidPatientId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/appointments/patient/{invalidPatientId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAppointmentsByDentist_WithInvalidDentistId_ShouldReturnNotFound()
    {
        // Arrange
        await SeedDatabaseAsync();
        var invalidDentistId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/appointments/dentist/{invalidDentistId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAppointment_WithInvalidData_ShouldReturnNotFound()
    {
        // Arrange
        await SeedDatabaseAsync();
        
        var createRequest = new AddAppointmentRequest
        {
            PatientId = Guid.NewGuid(), // Несуществующий пациент
            DentistId = Guid.NewGuid(), // Несуществующий дантист
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = TimeSpan.FromHours(1)
        };
        
        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/appointments", content);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelAppointment_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await SeedDatabaseAsync();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.PostAsync($"/api/appointments/{invalidId}/cancel", new StringContent(""));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
} 