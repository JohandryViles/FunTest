using System.Net;
using System.Net.Http.Json;
using Api.Application.Features.Usuarios.Queries;
using Api.Infrastructure.Data.Persistence;
using Api.Tests;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Queries;

public sealed class GetUsuarioByIdQueryTests
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
    public async Task GetUsuarioById_via_api_returns_user_when_exists()
    {
        var response = await _client.GetAsync($"/api/usuarios/{_seededUsuarioId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UsuarioDto>();

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(_seededUsuarioId);
        result.Nombre.ShouldNotBeNullOrWhiteSpace();
        result.Apellido.ShouldNotBeNullOrWhiteSpace();
        result.Email.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task GetUsuarioById_via_api_returns_not_found_when_user_does_not_exist()
    {
        var response = await _client.GetAsync($"/api/usuarios/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetUsuarioById_via_api_empty_guid_returns_bad_request()
    {
        var response = await _client.GetAsync($"/api/usuarios/{Guid.Empty}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetUsuarioById_via_mediatr_returns_user_when_exists()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new GetUsuarioByIdQuery(_seededUsuarioId));

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(_seededUsuarioId);
    }

    [Test]
    public async Task GetUsuarioById_via_mediatr_returns_null_when_user_does_not_exist()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new GetUsuarioByIdQuery(Guid.NewGuid()));

        result.ShouldBeNull();
    }

    [Test]
    public async Task GetUsuarioById_via_mediatr_empty_guid_throws_validation_exception()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Should.ThrowAsync<ValidationException>(() =>
            sender.Send(new GetUsuarioByIdQuery(Guid.Empty))
        );
    }
}
