using Domain.Models.Events;
using Shared.Core.Helpers;

namespace Domain.Entities;

public class Rental : BaseEntity
{
    protected Rental()
    {
    }

    private Rental(Guid motorcycleId, Guid riderId, Guid rentalPlanId, DateOnly startDate, DateOnly endDate,
        DateOnly expectedEndDate, DateOnly? returnDate = null, decimal? expectedAmount = null)
    {
        MotorcycleId = motorcycleId;
        RiderId = riderId;
        RentalPlanId = rentalPlanId;
        StartDate = startDate;
        EndDate = endDate;
        ExpectedEndDate = expectedEndDate;
        ReturnDate = returnDate;
        ExpectedAmount = expectedAmount;
    }

    public Guid MotorcycleId { get; private set; }
    public Guid RiderId { get; private set; }
    public Guid RentalPlanId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public DateOnly ExpectedEndDate { get; private set; }
    public DateOnly? ReturnDate { get; private set; }
    public decimal? ExpectedAmount { get; private set; }
    public decimal? TotalAmount { get; private set; }
    public decimal? PenaltyAmount { get; private set; }

    public Motorcycle Motorcycle { get; private set; }
    public Rider Rider { get; private set; }
    public RentalPlan RentalPlan { get; private set; }

    public static Result<Rental> Create(Guid motorcycleId, Guid riderId, Guid rentalPlanId, DateOnly startDate,
        DateOnly endDate, DateOnly expectedEndDate)
    {
        if (startDate > endDate)
            return Result.Failure<Rental>(Error.Validation("RENTAL.START_DATE.GREATER_THAN_END",
                "Data inicial não pode ser maior que a final"));

        if (startDate < DateOnly.FromDateTime(DateTime.Today))
            return Result.Failure<Rental>(Error.Validation("RENTAL.START_DATE.PAST_DATE",
                "Data inicial precisa ser um dia após hoje"));

        if (endDate > expectedEndDate)
            return Result.Failure<Rental>(Error.Validation("RENTAL.END_DATE.GREATER_THAN_EXPECTED_END",
                "Data final não pode ser maior que a data final prevista"));

        var rental = new Rental(motorcycleId, riderId, rentalPlanId, startDate, endDate, expectedEndDate);

        rental.AddDomainEvent(new RentalCreatedEvent(rental.Id, rentalPlanId));
        return Result.Success(rental);
    }

    public void UpdateExpectedAmount(decimal expectedAmount)
    {
        ExpectedAmount = expectedAmount;
    }

    public void UpdateTotalAmountAndPenalty(decimal totalAmount, decimal penaltyAmount)
    {
        TotalAmount = totalAmount;
        PenaltyAmount = penaltyAmount;
    }

    public Result SetReturnDate(DateOnly returnDate)
    {
        ReturnDate = returnDate;
        AddDomainEvent(new RentalReturnDateUpdatedEvent(Id, returnDate));
        return Result.Success();
    }
}