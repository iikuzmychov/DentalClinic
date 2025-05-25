using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Aggregates.AppointmentServiceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DentalClinic.Infrastructure.Configurations;

internal sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(appointment => appointment.Id);

        builder
            .HasIndex(appointment => appointment.StartTime)
            .IsUnique(false);

        builder
            .HasOne(appointment => appointment.Patient)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(appointment => appointment.Dentist)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(appointment => appointment.ProvidedServices)
            .WithMany()
            .UsingEntity<AppointmentService>(
                appointmentServiceToServiceBuilder => appointmentServiceToServiceBuilder
                    .HasOne(appointmentService => appointmentService.Service)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict),
                appointmentServiceToAppointmentBuilder => appointmentServiceToAppointmentBuilder
                    .HasOne(appointmentService => appointmentService.Appointment)
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict),
                providedServiceBuilder =>
                {
                    providedServiceBuilder.HasKey(service => service.Id);
                });
    }
}
