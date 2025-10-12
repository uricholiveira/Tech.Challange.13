using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class RentalRepository(ILogger<Repository<Rental>> logger, DatabaseContext context)
    : Repository<Rental>(logger, context), IRentalRepository;