using Testcontainers.MySql;
using DotNet.Testcontainers.Builders;
using Xunit.Sdk;

namespace LandEase.Tests.Integration;

public sealed class MySqlContainerFixture : IAsyncLifetime
{
    private MySqlContainer? _container;

    public string ConnectionString =>
        _container?.GetConnectionString()
        ?? throw new InvalidOperationException("Container is not initialized.");

    public async Task InitializeAsync()
    {
        try
        {
            _container = new MySqlBuilder("mysql:8.0")
                .WithDatabase("landease_test")
                .WithUsername("landease")
                .WithPassword("landease_password")
                .Build();

            await _container.StartAsync();
        }
        catch (DockerUnavailableException)
        {
            throw SkipException.ForSkip(
                "Integration tests skipped: Docker is not available/running on this machine.");
        }
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }
}

