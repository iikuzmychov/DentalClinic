using DentalClinic.Domain;
using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace DentalClinic.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connectionString);

        services.AddDbContext<ApplicationDbContext>(options => options
            .ReplaceService<IValueConverterSelector, ApplicationValueConverterSelector>()
            .UseSqlServer(connectionString)
            .UseSeeding((dbContext, _) =>
                SeedDatabaseAsync((ApplicationDbContext)dbContext, CancellationToken.None).Wait())
            .UseAsyncSeeding((dbContext, _, cancellationToken) =>
                SeedDatabaseAsync((ApplicationDbContext)dbContext, cancellationToken)));

        return services;
    }

    private static async Task SeedDatabaseAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var adminUser = new User
        {
            FirstName = "Адміністратор",
            LastName = "Клініки",
            Role = Role.Admin,
            Email = new Email("admin@dental.clinic"),
            HashedPassword = HashedPassword.FromSecurePassword(new SecurePassword("passW0RD!")),
        };

        await dbContext.Users.AddAsync(adminUser, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
