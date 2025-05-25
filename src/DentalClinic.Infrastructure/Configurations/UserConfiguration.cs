using DentalClinic.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalClinic.Infrastructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);

        builder
            .HasIndex(user => user.FirstName)
            .IsUnique(false);

        builder
            .HasIndex(user => user.LastName)
            .IsUnique(false);

        builder
            .HasIndex(user => user.Surname)
            .IsUnique(false);

        builder
            .HasIndex(user => user.Email)
            .IsUnique(true);
        
        builder.OwnsOne(user => user.HashedPassword);
    }
}
