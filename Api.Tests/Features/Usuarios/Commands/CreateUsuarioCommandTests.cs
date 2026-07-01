using System.Net;
using System.Net.Http.Json;
using Api.Application.Features.Usuarios.Commands;
using Api.Application.Features.Usuarios.Queries;
using Api.Infrastructure.Data.Persistence;
using Api.Tests;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Commands;

public sealed class CreateUsuarioCommandTests
{
    private HttpClient _client = null!;

    [SetUp]
    public async Task SetUp()
    {
        await TestHost.ResetDatabaseAsync();
        _client = TestHost.WebFactory.CreateClient();
    }

    [Test]
    public async Task CreateUsuario_via_api_persists_and_returns_created_user()
    {
        var command = new CreateUsuarioCommand("María", "García", "maria@example.com");

        var response = await _client.PostAsJsonAsync("/api/usuarios", command);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UsuarioDto>();

        result.ShouldNotBeNull();
        result!.Id.ShouldNotBe(Guid.Empty);
        result.Nombre.ShouldBe("María");
        result.Apellido.ShouldBe("García");
        result.Email.ShouldBe("maria@example.com");

        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Usuarios.CountAsync()).ShouldBe(1);
    }

    [Test]
    public async Task CreateUsuario_via_mediatr_persists_user()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(
            new CreateUsuarioCommand("Juan", "Pérez", "juan@example.com")
        );

        result.Nombre.ShouldBe("Juan");
        result.Apellido.ShouldBe("Pérez");
        result.Email.ShouldBe("juan@example.com");
    }

    [Test]
    public async Task CreateUsuario_trims_name_fields()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(
            new CreateUsuarioCommand("  Ana  ", "  López  ", "ana@example.com")
        );

        result.Nombre.ShouldBe("Ana");
        result.Apellido.ShouldBe("López");
    }

    [TestCase("", "García", "maria@example.com")]
    [TestCase("María", "", "maria@example.com")]
    [TestCase("María", "García", "")]
    public async Task CreateUsuario_via_api_missing_required_fields_returns_bad_request(
        string nombre,
        string apellido,
        string email
    )
    {
        var response = await _client.PostAsJsonAsync(
            "/api/usuarios",
            new CreateUsuarioCommand(nombre, apellido, email)
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [TestCase("not-an-email")]
    [TestCase("missing-at-sign.com")]
    [TestCase("@missing-local.com")]
    public async Task CreateUsuario_via_api_invalid_email_returns_bad_request(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/usuarios",
            new CreateUsuarioCommand("María", "García", email)
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [TestCase("", "García", "maria@example.com")]
    [TestCase("María", "", "maria@example.com")]
    [TestCase("María", "García", "invalid-email")]
    public async Task CreateUsuario_via_mediatr_invalid_input_throws_validation_exception(
        string nombre,
        string apellido,
        string email
    )
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Should.ThrowAsync<ValidationException>(() =>
            sender.Send(new CreateUsuarioCommand(nombre, apellido, email))
        );
    }

    [Test]
    public async Task CreateUsuario_via_api_whitespace_only_fields_returns_bad_request()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/usuarios",
            new CreateUsuarioCommand("   ", "   ", "   ")
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
