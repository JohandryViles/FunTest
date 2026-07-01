using Api.Infrastructure.Data.Persistence;
using Api.Infrastructure.Data.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<AppDbContext>("bd");

        return builder;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();
        await UsuarioSeeder.SeedAsync(db);
    }
}
