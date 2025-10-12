using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class RentalPlanRepository(ILogger<Repository<RentalPlan>> logger, DatabaseContext context)
    : Repository<RentalPlan>(logger, context), IRentalPlanRepository;