using System.Reflection;
using Infrastructure.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Helpers;

namespace Infrastructure.Data;

public class DatabaseContext(DbContextOptions<DatabaseContext> options, IMediator mediator)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tech_challange");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetUpdateAndCreatedDateTimeOnChangedDbEntities();
        var result = base.SaveChanges(acceptAllChangesOnSuccess);

        DomainEventDispatcher.DispatchEventsAsync(this, mediator).GetAwaiter().GetResult();
        return result;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        SetUpdateAndCreatedDateTimeOnChangedDbEntities();

        var result = await base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken
        );

        await DomainEventDispatcher.DispatchEventsAsync(this, mediator);
        return result;
    }

    private void SetUpdateAndCreatedDateTimeOnChangedDbEntities()
    {
        var entities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entity in entities)
            switch (entity.State)
            {
                case EntityState.Added:
                    entity.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entity.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
    }
}