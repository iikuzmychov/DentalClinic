using DentalClinic.Domain.Aggregates.UserAggregate;
using DentalClinic.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Infrastructure;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.ApplyConversionsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
