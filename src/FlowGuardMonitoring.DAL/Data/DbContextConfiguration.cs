using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowGuardMonitoring.DAL.Data;

public static class DbContextConfiguration
{
    public static void AddFlowGuardMonitoringContext(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<FlowGuardMonitoringContext>(options =>
            options.UseSqlServer(connectionString));
    }
}