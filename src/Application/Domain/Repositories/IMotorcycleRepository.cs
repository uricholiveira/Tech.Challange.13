using Domain.Entities;
using Shared.Core.Helpers;

namespace Domain.Repositories;

public interface IMotorcycleRepository : IRepository<Motorcycle>
{
    public Task<Result<List<Motorcycle>>> ListByParams(string? licensePlate = null, string? model = null,
        int? year = null, CancellationToken cancellationToken = default);
}