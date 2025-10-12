using Domain.Models.Events;
using Shared.Core.Helpers;

namespace Domain.Entities;

public class Motorcycle : BaseEntity
{
    private readonly List<Rental> _rentals;

    protected Motorcycle()
    {
    }

    private Motorcycle(string identifier, int year, string model, string licensePlate)
    {
        Identifier = identifier;
        Year = year;
        Model = model;
        LicensePlate = licensePlate.ToUpper();
    }

    public string Identifier { get; private set; }
    public int Year { get; private set; }
    public string Model { get; private set; }
    public string LicensePlate { get; private set; }
    public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();

    public static Result<Motorcycle> Create(string identifier, int year, string model, string licensePlate)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return Result.Failure<Motorcycle>(Error.Validation("MOTORCYCLE.IDENTIFIER.EMPTY",
                "Identificador não pode ser vazio"));

        if (string.IsNullOrWhiteSpace(model))
            return Result.Failure<Motorcycle>(Error.Validation("MOTORCYCLE.MODEL.EMPTY",
                "Modelo não pode ser vazio"));

        if (string.IsNullOrWhiteSpace(licensePlate))
            return Result.Failure<Motorcycle>(Error.Validation("MOTORCYCLE.LICENSE_PLATE.EMPTY",
                "Placa não pode ser vazia"));

        if (year < 1900)
            return Result.Failure<Motorcycle>(Error.Validation("MOTORCYCLE.YEAR.LESS_THAN_1900",
                "Ano da moto não pode ser menor que 1900"));
        if (year > DateTime.UtcNow.Year)
            return Result.Failure<Motorcycle>(Error.Validation("MOTORCYCLE.YEAR.GREATER_THAN_CURRENT",
                "Ano da moto não pode ser maior que o ano atual"));

        var motorcycle = new Motorcycle(identifier, year, model, licensePlate);
        motorcycle.AddDomainEvent(new MotorcycleCreatedEvent(motorcycle.Id, identifier, year, model, licensePlate));
        return Result.Success(motorcycle);
    }

    public void UpdateLicensePlate(string licensePlate)
    {
        LicensePlate = licensePlate.ToUpper();
    }
}