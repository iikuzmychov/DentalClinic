using DentalClinic.Domain.Aggregates.AppointmentAggregate;
using DentalClinic.Domain.Aggregates.ServiceAggregate;
using DentalClinic.Domain.Types;
using System.ComponentModel.DataAnnotations.Schema;

namespace DentalClinic.Domain.Aggregates.AppointmentServiceAggregate;

public sealed class AppointmentService
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public GuidEntityId<AppointmentService> Id { get; init; } = GuidEntityId<AppointmentService>.New();

    public Service Service { get; private set; }
    public Appointment Appointment { get; private set; }

    public AppointmentService(Service service, Appointment appointment)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(appointment);

        Service = service;
        Appointment = appointment;
    }

    [Obsolete("This constructor is for EF Core only.")]
    private AppointmentService()
    {
        Service = default!;
        Appointment = default!;
    }
}
