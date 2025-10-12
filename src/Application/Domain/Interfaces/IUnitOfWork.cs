using Shared.Core.Helpers;

namespace Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<Result<int>> SaveChangesAsync();
    Task<Result> BeginTransactionAsync();
    Task<Result> CommitTransactionAsync();
    Task<Result> RollbackTransactionAsync();
}