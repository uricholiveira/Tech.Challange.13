namespace Shared.Core.Helpers;

public class BaseEntity
{
    private readonly List<object> _domainEvents = [];
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(object @event)
    {
        _domainEvents.Add(@event);
    }

    public void RemoveDomainEvent(object @event)
    {
        _domainEvents.Remove(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}