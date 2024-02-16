using AuctionService.Data;
using AuctionService.Entities;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AuctionService.IntegrationTests.Fixtures;

// The test instance of webapplication by AspNetCore.Mvc.Testing
// Implement IAsyncLifetim which is from xunit, to create test container when test starting.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    // for testing begin
    public async Task InitializeAsync()
    {
        // runing postgresql testing container
        await _postgreSqlContainer.StartAsync();
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // this will initial service in Program, then we can change it
        // with testing one.
        builder.ConfigureTestServices(services =>
        {
            // replace PostgreSQL
            services.RemoveDbContext<Auction>();

            services.AddDbContext<AuctionDbContext>(opt =>
            {
                opt.UseNpgsql(_postgreSqlContainer.GetConnectionString());
            });


            // replace Masstransit, using Masstransit built-in functionality
            services.AddMassTransitTestHarness(); // don't do something else

            services.InitTestingDb<AuctionDbContext>();
        });
    }

    // for testing end
    Task IAsyncLifetime.DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();
}
