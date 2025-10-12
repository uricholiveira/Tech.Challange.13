using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Helpers;

namespace Infrastructure.Helpers;

public static class DomainEventDispatcher
{
    public static async Task DispatchEventsAsync<T>(T context, IMediator mediator)
        where T : DbContext
    {
        var entitiesWithEvents = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        entitiesWithEvents.ForEach(e => e.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents) await mediator.Publish(domainEvent);
    }
}