using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Bogus;

namespace Api.Tests.Factories;

internal static class UsuarioFactory
{
    private static readonly Faker Faker = new("es");

    public static Usuario Create() =>
        new()
        {
            Id = UsuarioId.FromNewGuid(),
            Nombre = Faker.Name.FirstName(),
            Apellido = Faker.Name.LastName(),
            Email = Email.From(Faker.Internet.Email()),
        };

    public static IReadOnlyList<Usuario> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
