using MediatR;

namespace Shared.Core.Abstracts;

public abstract class DomainEvent : INotification
{
    public Event Event { get; } = new();
}

public sealed class Event
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}