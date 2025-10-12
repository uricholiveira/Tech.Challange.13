using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Infrastructure.Repositories;

public class MotorcycleRepository(ILogger<Repository<Motorcycle>> logger, DatabaseContext context)
    : Repository<Motorcycle>(logger, context), IMotorcycleRepository
{
    private readonly ILogger<Repository<Motorcycle>> _logger = logger;

    public async Task<Result<List<Motorcycle>>> ListByParams(string? licensePlate = null, string? model = null,
        int? year = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(licensePlate))
                query = query.Where(x => x.LicensePlate == licensePlate);

            if (!string.IsNullOrWhiteSpace(model))
                query = query.Where(x => x.Model == model);

            if (year.HasValue)
                query = query.Where(x => x.Year == year.Value);

            var result = await query.ToListAsync(cancellationToken);
            return Result.Success(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao listar motos");
            return Result.Failure<List<Motorcycle>>(Error.Failure("MOTORCYCLE_LIST_ERROR",
                "Erro ao listar motos"));
        }
    }
}