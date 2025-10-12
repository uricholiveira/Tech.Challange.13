using Shared.Core.Helpers;

namespace Domain.Entities;

public class RentalPlan : BaseEntity
{
    private readonly List<Rental> _rentals;

    protected RentalPlan()
    {
    }

    private RentalPlan(Guid id, int days, decimal dailyAmount, decimal penaltyPercentage)
    {
        Id = id;
        Days = days;
        DailyAmount = dailyAmount;
        PenaltyPercentage = penaltyPercentage;
    }

    public int Days { get; private set; }
    public decimal DailyAmount { get; private set; }
    public decimal PenaltyPercentage { get; private set; }
    public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();

    public static Result<RentalPlan> Create(Guid id, int days, decimal dailyAmount, decimal penaltyPercentage)
    {
        if (days <= 0)
            return Result.Failure<RentalPlan>(
                Error.Validation("RENTAL.DAYS.MUST_BE_POSITIVE", "Quantidade de dias deve ser positiva"));

        if (dailyAmount <= 0)
            return Result.Failure<RentalPlan>(Error.Validation("RENTAL.DAILY_AMOUNT.MUST_BE_POSITIVE",
                "Valor diário deve ser positivo"));

        return penaltyPercentage < 0
            ? Result.Failure<RentalPlan>(Error.Validation("RENTAL.PENALTY_PERCENTAGE.MUST_BE_NON_NEGATIVE",
                "Percentual de multa deve ser não negativo"))
            : Result.Success(new RentalPlan(id, days, dailyAmount, penaltyPercentage));
    }
}