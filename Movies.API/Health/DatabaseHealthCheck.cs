using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.API.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connection;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IDbConnectionFactory connection, ILogger<DatabaseHealthCheck> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            using var _ = await _connection.CreateConnectionAsync();
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            _logger.LogError("database unhealthy", e);
            return HealthCheckResult.Unhealthy("database unhealthy");
        }
    }
}