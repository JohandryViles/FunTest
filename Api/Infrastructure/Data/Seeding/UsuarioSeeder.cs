using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Data.Persistence;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Data.Seeding;

public static class UsuarioSeeder
{
    public const int DefaultCount = 25;

    public static async Task SeedAsync(
        AppDbContext db,
        int count = DefaultCount,
        CancellationToken cancellationToken = default
    )
    {
        if (await db.Usuarios.AnyAsync(cancellationToken))
            return;

        var faker = new Faker("es");

        var usuarios = Enumerable
            .Range(0, count)
            .Select(_ => new Usuario
            {
                Id = UsuarioId.FromNewGuid(),
                Nombre = faker.Name.FirstName(),
                Apellido = faker.Name.LastName(),
                Email = Email.From(faker.Internet.Email()),
            })
            .ToList();

        db.Usuarios.AddRange(usuarios);
        await db.SaveChangesAsync(cancellationToken);
    }
}
