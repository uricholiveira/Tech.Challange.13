using Shared.Core.Abstracts;

namespace Domain.Models.Events;

public class MotorcycleCreatedEvent(Guid id, string identifier, int year, string model, string licensePlate)
    : DomainEvent
{
    public Guid Id { get; set; } = id;
    public string Identifier { get; set; } = identifier;
    public int Year { get; set; } = year;
    public string Model { get; set; } = model;
    public string LicensePlate { get; set; } = licensePlate;
}