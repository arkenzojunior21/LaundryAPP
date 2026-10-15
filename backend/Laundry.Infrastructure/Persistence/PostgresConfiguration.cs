using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Laundry.Infrastructure.Persistence;

public static class PostgresConfiguration
{
    public static IServiceCollection AddLaundryPostgres(this IServiceCollection services, string connectionString) =>
        services.AddDbContext<LaundryDbContext>(options => options.UseNpgsql(connectionString));
}
