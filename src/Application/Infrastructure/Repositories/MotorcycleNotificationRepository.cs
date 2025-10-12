using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class MotorcycleNotificationRepository(
    ILogger<Repository<MotorcycleNotification>> logger,
    DatabaseContext context)
    : Repository<MotorcycleNotification>(logger, context), IMotorcycleNotificationRepository;