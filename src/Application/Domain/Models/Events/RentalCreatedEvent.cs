using Shared.Core.Abstracts;

namespace Domain.Models.Events;

public class RentalCreatedEvent(Guid id, Guid rentalPlanId)
    : DomainEvent
{
    public Guid Id { get; } = id;
    public Guid RentalPlanId { get; } = rentalPlanId;
}