using ECommerce_Project.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Abstractions;

public class FunctionalTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder("postgres:15.1").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ECommerceDbContext>>();
            services.AddDbContext<ECommerceDbContext>(options =>
            {
                options.UseNpgsql(postgreSqlContainer.GetConnectionString())
                    .UseSnakeCaseNamingConvention();
            });

        });
    }

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        await postgreSqlContainer.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await postgreSqlContainer.StopAsync();
        await postgreSqlContainer.DisposeAsync();

        await base.DisposeAsync();
    }
}
