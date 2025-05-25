using DentalClinic.Domain.Aggregates.PatientAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalClinic.Infrastructure.Configurations;

internal sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(patient => patient.Id);

        builder
            .HasIndex(patient => patient.FirstName)
            .IsUnique(false);

        builder
            .HasIndex(patient => patient.LastName)
            .IsUnique(false);

        builder
            .HasIndex(patient => patient.Surname)
            .IsUnique(false);

        builder
            .HasIndex(patient => patient.Email)
            .IsUnique(true);
    }
}
