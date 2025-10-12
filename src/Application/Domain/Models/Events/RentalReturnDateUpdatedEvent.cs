using Shared.Core.Abstracts;

namespace Domain.Models.Events;

public class RentalReturnDateUpdatedEvent(Guid id, DateOnly returnDate)
    : DomainEvent
{
    public Guid Id { get; } = id;
    public DateOnly ReturnDate { get; } = returnDate;
}