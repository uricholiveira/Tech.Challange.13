using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class RiderRepository(ILogger<Repository<Rider>> logger, DatabaseContext context)
    : Repository<Rider>(logger, context), IRiderRepository;