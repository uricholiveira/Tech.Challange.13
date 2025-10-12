using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Infrastructure.Data;

public class UnitOfWork(ILogger<UnitOfWork> logger, DatabaseContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task<Result<int>> SaveChangesAsync()
    {
        try
        {
            var changes = await context.SaveChangesAsync();
            return Result.Success(changes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao salvar mudanças");
            return Result.Failure<int>(Error.Failure("DB_SAVE_CHANGES", "Erro ao salvar mudanças"));
        }
    }

    public async Task<Result> BeginTransactionAsync()
    {
        try
        {
            _transaction = await context.Database.BeginTransactionAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao iniciar transação");
            return Result.Failure(Error.Failure("DB_TRANSACTION_START", "Erro ao iniciar transação"));
        }
    }

    public async Task<Result> CommitTransactionAsync()
    {
        try
        {
            if (_transaction == null)
                return Result.Failure(Error.Failure("DB_TRANSACTION_COMMIT", "Nenhuma transação ativa para commit"));

            await _transaction.CommitAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao commitar transação");
            return Result.Failure(Error.Failure("DB_TRANSACTION_COMMIT", "Erro ao commitar transação"));
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task<Result> RollbackTransactionAsync()
    {
        try
        {
            if (_transaction == null)
                return Result.Failure(Error.Failure("DB_TRANSACTION_ROLLBACK",
                    "Nenhuma transação ativa para rollback"));

            await _transaction.RollbackAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao fazer rollback");
            return Result.Failure(Error.Failure("DB_TRANSACTION_ROLLBACK", "Erro ao fazer rollback"));
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _transaction?.Dispose();
    }
}