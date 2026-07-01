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

public sealed class UpdateUsuarioCommandTests
{
    private HttpClient _client = null!;
    private Guid _seededUsuarioId;

    [SetUp]
    public async Task SetUp()
    {
        await TestHost.ResetAndSeedUsuariosAsync(1);

        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _seededUsuarioId = (await db.Usuarios.AsNoTracking().SingleAsync()).Id.Value;

        _client = TestHost.WebFactory.CreateClient();
    }

    [Test]
    public async Task UpdateUsuario_via_api_updates_and_returns_user()
    {
        var request = new UpdateUsuarioRequest("María", "García", "maria.nueva@example.com");

        var response = await _client.PutAsJsonAsync($"/api/usuarios/{_seededUsuarioId}", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UsuarioDto>();

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(_seededUsuarioId);
        result.Nombre.ShouldBe("María");
        result.Apellido.ShouldBe("García");
        result.Email.ShouldBe("maria.nueva@example.com");
    }

    [Test]
    public async Task UpdateUsuario_via_mediatr_persists_changes()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(
            new UpdateUsuarioCommand(_seededUsuarioId, "Juan", "Pérez", "juan@example.com")
        );

        result.ShouldNotBeNull();
        result!.Nombre.ShouldBe("Juan");
        result.Apellido.ShouldBe("Pérez");
        result.Email.ShouldBe("juan@example.com");
    }

    [Test]
    public async Task UpdateUsuario_trims_name_fields()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(
            new UpdateUsuarioCommand(_seededUsuarioId, "  Ana  ", "  López  ", "ana@example.com")
        );

        result.ShouldNotBeNull();
        result!.Nombre.ShouldBe("Ana");
        result.Apellido.ShouldBe("López");
    }

    [Test]
    public async Task UpdateUsuario_via_api_returns_not_found_when_user_does_not_exist()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/usuarios/{Guid.NewGuid()}",
            new UpdateUsuarioRequest("María", "García", "maria@example.com")
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateUsuario_via_mediatr_returns_null_when_user_does_not_exist()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(
            new UpdateUsuarioCommand(Guid.NewGuid(), "María", "García", "maria@example.com")
        );

        result.ShouldBeNull();
    }

    [Test]
    public async Task UpdateUsuario_via_api_returns_not_found_for_soft_deleted_user()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        (await sender.Send(new DeleteUsuarioCommand(_seededUsuarioId))).ShouldBeTrue();

        var response = await _client.PutAsJsonAsync(
            $"/api/usuarios/{_seededUsuarioId}",
            new UpdateUsuarioRequest("María", "García", "maria@example.com")
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateUsuario_via_api_empty_guid_returns_bad_request()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/usuarios/{Guid.Empty}",
            new UpdateUsuarioRequest("María", "García", "maria@example.com")
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [TestCase("", "García", "maria@example.com")]
    [TestCase("María", "", "maria@example.com")]
    [TestCase("María", "García", "invalid-email")]
    public async Task UpdateUsuario_via_api_invalid_input_returns_bad_request(
        string nombre,
        string apellido,
        string email
    )
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/usuarios/{_seededUsuarioId}",
            new UpdateUsuarioRequest(nombre, apellido, email)
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task UpdateUsuario_via_mediatr_empty_guid_throws_validation_exception()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Should.ThrowAsync<ValidationException>(() =>
            sender.Send(
                new UpdateUsuarioCommand(Guid.Empty, "María", "García", "maria@example.com")
            )
        );
    }
}
