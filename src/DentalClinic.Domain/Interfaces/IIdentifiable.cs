namespace DentalClinic.Domain.Interfaces;

public interface IIdentifiable<TId>
{
    public TId Id { get; }
}
