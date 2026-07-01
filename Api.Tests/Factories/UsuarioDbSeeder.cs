using Api.Infrastructure.Data.Persistence;

namespace Api.Tests.Factories;

internal static class UsuarioDbSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        int count,
        CancellationToken cancellationToken = default
    )
    {
        var usuarios = UsuarioFactory.CreateMany(count);

        db.Usuarios.AddRange(usuarios);
        await db.SaveChangesAsync(cancellationToken);
    }
}
