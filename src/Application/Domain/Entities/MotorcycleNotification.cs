using Shared.Core.Helpers;

namespace Domain.Entities;

public class MotorcycleNotification : BaseEntity
{
    protected MotorcycleNotification()
    {
    }

    private MotorcycleNotification(Guid motorcycleId, int year, string model, string licensePlate)
    {
        MotorcycleId = motorcycleId;
        Year = year;
        Model = model;
        LicensePlate = licensePlate;
        NotificationDate = DateTime.UtcNow;
    }

    public Guid MotorcycleId { get; private set; }
    public int Year { get; private set; }
    public string Model { get; private set; }
    public string LicensePlate { get; private set; }
    public DateTime NotificationDate { get; private set; }

    public static Result<MotorcycleNotification> Create(Guid motorcycleId, int year, string model, string licensePlate)
    {
        return Result.Success(new MotorcycleNotification(motorcycleId, year, model, licensePlate));
    }
}