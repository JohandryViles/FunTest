using Api.Infrastructure.Data.Persistence;
using Api.Tests.Factories;
using Api.Tests.Infrastructure;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Projects;
using Respawn;

namespace Api.Tests;

[SetUpFixture]
public sealed class TestHost
{
    public static DistributedApplication App { get; private set; } = null!;

    public static string ConnectionString { get; private set; } = null!;

    public static ApiWebApplicationFactory WebFactory { get; private set; } = null!;

    public static Respawner Respawner { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Api_Tests_AppHost>([]);

        App = await appHost.BuildAsync();
        await App.StartAsync();

        await App.ResourceNotifications.WaitForResourceHealthyAsync("bdserver");

        ConnectionString =
            await App.GetConnectionStringAsync("bd")
            ?? throw new InvalidOperationException("No se encontró la connection string 'bd'.");

        await WaitForDatabaseAsync(ConnectionString);

        WebFactory = new ApiWebApplicationFactory(ConnectionString);

        await using var scope = WebFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        Respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                TablesToIgnore = ["__EFMigrationsHistory"],
            }
        );
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await WebFactory.DisposeAsync();
        await App.DisposeAsync();
    }

    public static async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await Respawner.ResetAsync(connection);
    }

    public static async Task SeedUsuariosAsync(int count)
    {
        await using var scope = WebFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await UsuarioDbSeeder.SeedAsync(db, count);
    }

    public static async Task ResetAndSeedUsuariosAsync(int count = 15)
    {
        await ResetDatabaseAsync();
        await SeedUsuariosAsync(count);
    }

    private static async Task WaitForDatabaseAsync(string connectionString)
    {
        for (var attempt = 0; attempt < 60; attempt++)
        {
            try
            {
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                return;
            }
            catch (SqlException) when (attempt < 59)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        throw new InvalidOperationException(
            "La base de datos de tests no estuvo disponible a tiempo."
        );
    }
}
