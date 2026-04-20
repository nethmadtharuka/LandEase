using LandEase.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LandEase.Tests.Integration;

public sealed class MySqlApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public MySqlApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["JwtSettings:Secret"] = "TEST_ONLY_SUPER_SECRET_CHANGE_ME",
                ["JwtSettings:Issuer"] = "LandEaseAPI",
                ["JwtSettings:Audience"] = "LandEaseClient",
                ["JwtSettings:ExpiryMinutes"] = "60",
                ["RateLimiting:AiEndpointsPerMinute"] = "1000"
            };

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            // Ensure DB schema exists for tests.
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        });
    }
}

