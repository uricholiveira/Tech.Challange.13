using System.Linq.Expressions;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Infrastructure.Repositories;

public class Repository<T>(ILogger<Repository<T>> logger, DatabaseContext context) : IRepository<T>
    where T : class
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<Result<T>> GetByIdAsync(int id)
    {
        try
        {
            var entity = await DbSet.FindAsync(id);
            return entity == null
                ? Result.Failure<T>(Error.NotFound("DB_NOT_FOUND", $"Entidade com ID {id} não encontrada"))
                : Result.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar entidade");
            return Result.Failure<T>(Error.Failure("DB_GET_BY_ID", "Erro ao buscar entidade"));
        }
    }

    public virtual async Task<Result<IEnumerable<T>>> GetAllAsync()
    {
        try
        {
            var entities = await DbSet.ToListAsync();
            return Result.Success<IEnumerable<T>>(entities);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao listar entidades");
            return Result.Failure<IEnumerable<T>>(Error.Failure("DB_LIST_ALL", "Erro ao listar entidades"));
        }
    }

    public virtual async Task<Result<IEnumerable<T>>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var entities = await DbSet.Where(predicate).ToListAsync();
            return Result.Success<IEnumerable<T>>(entities);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao filtrar entidades");
            return Result.Failure<IEnumerable<T>>(Error.Failure("DB_LIST_ALL", "Erro ao filtrar entidades"));
        }
    }

    public virtual async Task<Result<T>> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var entity = await DbSet.FirstOrDefaultAsync(predicate);
            return entity == null
                ? Result.Failure<T>(Error.NotFound("DB_NOT_FOUND",
                    "Entidade não encontrada")) // TODO: Inserir nome da entidade, ou retornar null
                : Result.Success(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar entidade");
            return Result.Failure<T>(Error.Failure("DB_GET_BY_ID", $"Erro ao buscar entidade: {ex.Message}"));
        }
    }

    public virtual void Add(T entity)
    {
        DbSet.Add(entity);
    }

    public virtual void AddRange(IEnumerable<T> entities)
    {
        DbSet.AddRange(entities);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public virtual async Task<Result> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await DbSet.AddAsync(entity, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao adicionar entidade");
            return Result.Failure(Error.Failure("DB_ADD", $"Erro ao adicionar entidade: {ex.Message}"));
        }
    }

    public virtual async Task<Result> AddRangeAsync(IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await DbSet.AddRangeAsync(entities, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao adicionar entidades");
            return Result.Failure(Error.Failure("DB_ADD", $"Erro ao adicionar entidades: {ex.Message}"));
        }
    }

    public virtual async Task<Result> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            DbSet.Update(entity);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar entidade");
            return Result.Failure(Error.Failure("DB_UPDATE", $"Erro ao atualizar entidade: {ex.Message}"));
        }
    }

    public virtual async Task<Result> RemoveAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            DbSet.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao remover entidade");
            return Result.Failure(Error.Failure("DB_REMOVE", $"Erro ao remover entidade: {ex.Message}"));
        }
    }

    public virtual async Task<Result> RemoveRangeAsync(IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        try
        {
            DbSet.RemoveRange(entities);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao remover entidades");
            return Result.Failure(Error.Failure("DB_REMOVE", $"Erro ao remover entidades: {ex.Message}"));
        }
    }

    public virtual async Task<Result<bool>> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var exists = await DbSet.AnyAsync(predicate);
            return Result.Success(exists);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao verificar existência");
            return Result.Failure<bool>(Error.Failure("DB_EXISTS", $"Erro ao verificar existência: {ex.Message}"));
        }
    }

    public virtual async Task<Result<int>> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        try
        {
            var count = predicate == null
                ? await DbSet.CountAsync()
                : await DbSet.CountAsync(predicate);
            return Result.Success(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao contar entidades");
            return Result.Failure<int>(Error.Failure("DB_COUNT", $"Erro ao contar entidades: {ex.Message}"));
        }
    }

    public virtual async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao salvar alterações");
            return Result.Failure(Error.Failure("DB_SAVE_CHANGES", $"Erro ao salvar alterações: {ex.Message}"));
        }
    }
}