using DentalClinic.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain;
using DentalClinic.Domain.Enums;

namespace DentalClinic.Tests.Integrational;

public class TestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected WebApplicationFactory<Program> Factory { get; }
    protected HttpClient Client { get; }

    public TestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var servicesToRemove = services
                    .Where(descriptor =>
                        descriptor.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>) ||
                        descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                        descriptor.ServiceType == typeof(DbContextOptions) ||
                        descriptor.ServiceType == typeof(ApplicationDbContext))
                    .ToList();

                foreach (var service in servicesToRemove)
                {
                    services.Remove(service);
                }

                services.AddDbContext<ApplicationDbContext>(options => options
                    .ReplaceService<IValueConverterSelector, ApplicationValueConverterSelector>()
                    .UseInMemoryDatabase("TestDatabase")
                    .UseProjectables());

                // Configure JWT for tests
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "dotnet-user-jwts",
                        ValidateAudience = true,
                        ValidAudiences = new[] { "https://localhost:7227" },
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = TestJwtHelper.GetTestSecurityKey(),
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = "role"
                    };
                });
            });
        });

        Client = CreateAuthorizedClient();
    }

    protected HttpClient CreateAuthorizedClient(
        string userId = "test-user-id",
        string email = "test@example.com", 
        string firstName = "Test",
        string lastName = "User",
        string role = "Admin")
    {
        var client = Factory.CreateClient();
        var token = TestJwtHelper.GenerateTestToken(userId, email, firstName, lastName, role);
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected async Task<ApplicationDbContext> GetDbContextAsync()
    {
        var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    protected async Task SeedDatabaseAsync()
    {
        using var context = await GetDbContextAsync();
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        // Create test data
        
        // Patients
        var patient1 = new Patient
        {
            FirstName = "Іван",
            LastName = "Петренко", 
            Surname = "Олександрович",
            Email = new Email("ivan.petrenko@example.com"),
            PhoneNumber = "+380123456789",
            Notes = "Test patient 1"
        };
        
        var patient2 = new Patient
        {
            FirstName = "Марія",
            LastName = "Іванова",
            Surname = "Петрівна", 
            Email = new Email("maria.ivanova@example.com"),
            PhoneNumber = "+380987654321",
            Notes = "Test patient 2"
        };
        
        context.Patients.AddRange(patient1, patient2);
        
        // Services
        var service1 = new Service
        {
            Name = "Чистка зубів",
            Price = new Price(500.00m)
        };
        
        var service2 = new Service  
        {
            Name = "Лікування карієсу",
            Price = new Price(800.00m)
        };
        
        context.Services.AddRange(service1, service2);
        
        // Users (dentists)
        var testHash = new byte[64]; // All zeros for testing
        var testSalt = new byte[64]; // All zeros for testing
        
        var dentist1 = new User
        {
            FirstName = "Олексій",
            LastName = "Коваленко",
            Surname = "Віталійович", 
            Email = new Email("dentist1@clinic.com"),
            Role = Role.Dentist,
            HashedPassword = new HashedPassword(testHash, testSalt)
        };
        
        var dentist2 = new User
        {
            FirstName = "Тетяна", 
            LastName = "Мельник",
            Surname = "Сергіївна",
            Email = new Email("dentist2@clinic.com"), 
            Role = Role.Dentist,
            HashedPassword = new HashedPassword(testHash, testSalt)
        };
        
        context.Users.AddRange(dentist1, dentist2);
        
        await context.SaveChangesAsync();
    }
} 
