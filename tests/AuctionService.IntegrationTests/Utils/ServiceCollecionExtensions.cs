using AuctionService.Data;
using AuctionService.IntegrationTests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

public static class ServiceCollecionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services)
    {
        // replace PostgreSQL
        var descriptor = services.SingleOrDefault(s =>
            s.ServiceType == typeof(DbContextOptions<AuctionDbContext>));

        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }
    }

    public static void EnsureCreated<T>(this IServiceCollection services)
    {
        // build the DI container with service instances, so we can
        // use it in the test
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        dbContext.Database.Migrate();
        DbHelper.InitDbForTest(dbContext);
    }
}
