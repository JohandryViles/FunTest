using System.Net;
using System.Net.Http.Json;
using Api.Application.Features.Usuarios.Commands;
using Api.Application.Features.Usuarios.Queries;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Data.Persistence;
using Api.Tests;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Commands;

public sealed class DeleteUsuarioCommandTests
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
    public async Task DeleteUsuario_via_api_returns_no_content()
    {
        var response = await _client.DeleteAsync($"/api/usuarios/{_seededUsuarioId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteUsuario_via_mediatr_soft_deletes_user()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var deleted = await sender.Send(new DeleteUsuarioCommand(_seededUsuarioId));

        deleted.ShouldBeTrue();
    }

    [Test]
    public async Task DeleteUsuario_excludes_user_from_list_and_get_by_id()
    {
        var deleteResponse = await _client.DeleteAsync($"/api/usuarios/{_seededUsuarioId}");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var listResponse = await _client.GetAsync("/api/usuarios");
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var list = await listResponse.Content.ReadFromJsonAsync<PaginatedUsuariosDto>();
        list.ShouldNotBeNull();
        list!.TotalCount.ShouldBe(0);

        var getResponse = await _client.GetAsync($"/api/usuarios/{_seededUsuarioId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteUsuario_keeps_row_in_database_with_deleted_at()
    {
        await _client.DeleteAsync($"/api/usuarios/{_seededUsuarioId}");

        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var usuario = await db
            .Usuarios.IgnoreQueryFilters()
            .SingleAsync(u => u.Id == UsuarioId.From(_seededUsuarioId));

        usuario.DeletedAt.ShouldNotBeNull();
    }

    [Test]
    public async Task DeleteUsuario_via_api_returns_not_found_when_user_does_not_exist()
    {
        var response = await _client.DeleteAsync($"/api/usuarios/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteUsuario_via_mediatr_returns_false_when_user_does_not_exist()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var deleted = await sender.Send(new DeleteUsuarioCommand(Guid.NewGuid()));

        deleted.ShouldBeFalse();
    }

    [Test]
    public async Task DeleteUsuario_via_api_returns_not_found_when_already_deleted()
    {
        var firstDelete = await _client.DeleteAsync($"/api/usuarios/{_seededUsuarioId}");
        firstDelete.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var secondDelete = await _client.DeleteAsync($"/api/usuarios/{_seededUsuarioId}");
        secondDelete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteUsuario_via_api_empty_guid_returns_bad_request()
    {
        var response = await _client.DeleteAsync($"/api/usuarios/{Guid.Empty}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task DeleteUsuario_via_mediatr_empty_guid_throws_validation_exception()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Should.ThrowAsync<ValidationException>(() =>
            sender.Send(new DeleteUsuarioCommand(Guid.Empty))
        );
    }
}
