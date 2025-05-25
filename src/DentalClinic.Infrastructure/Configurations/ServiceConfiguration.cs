using DentalClinic.Domain.Aggregates.ServiceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalClinic.Infrastructure.Configurations;

internal sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(service => service.Id);

        builder
            .HasIndex(service => service.Name)
            .IsUnique(true);
    }
}
