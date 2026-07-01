using System.Net;
using System.Net.Http.Json;
using Api.Application.Features.Usuarios.Queries;
using Api.Tests;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Queries;

public sealed class GetUsuariosQueryTests
{
    private const int SeedCount = 15;

    private HttpClient _client = null!;

    [SetUp]
    public async Task SetUp()
    {
        await TestHost.ResetAndSeedUsuariosAsync(SeedCount);
        _client = TestHost.WebFactory.CreateClient();
    }

    [Test]
    public async Task GetUsuarios_via_api_returns_paginated_result()
    {
        var response = await _client.GetAsync("/api/usuarios?page=1&pageSize=5");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedUsuariosDto>();

        result.ShouldNotBeNull();
        result!.Items.Count.ShouldBe(5);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(5);
        result.TotalCount.ShouldBe(SeedCount);
        result.TotalPages.ShouldBe(3);
    }

    [Test]
    public async Task GetUsuarios_via_mediatr_returns_paginated_result()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new GetUsuariosQuery(Page: 2, PageSize: 4));

        result.Items.Count.ShouldBe(4);
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(4);
        result.TotalCount.ShouldBe(SeedCount);
        result.TotalPages.ShouldBe(4);
    }

    [Test]
    public async Task GetUsuarios_last_page_returns_remaining_items()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new GetUsuariosQuery(Page: 3, PageSize: 5));

        result.Items.Count.ShouldBe(5);
        result.Page.ShouldBe(3);
        result.TotalCount.ShouldBe(SeedCount);
        result.TotalPages.ShouldBe(3);
    }

    [Test]
    public async Task GetUsuarios_page_beyond_total_returns_empty_items()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new GetUsuariosQuery(Page: 10, PageSize: 5));

        result.Items.ShouldBeEmpty();
        result.Page.ShouldBe(10);
        result.TotalCount.ShouldBe(SeedCount);
        result.TotalPages.ShouldBe(3);
    }

    [Test]
    public async Task GetUsuarios_with_empty_database_returns_zero_totals()
    {
        await TestHost.ResetDatabaseAsync();

        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new GetUsuariosQuery());

        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        result.TotalPages.ShouldBe(0);
    }

    [Test]
    public async Task GetUsuarios_page_size_one_returns_single_item()
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new GetUsuariosQuery(Page: 1, PageSize: 1));

        result.Items.Count.ShouldBe(1);
        result.PageSize.ShouldBe(1);
        result.TotalPages.ShouldBe(SeedCount);
    }

    [Test]
    public async Task GetUsuarios_page_size_at_maximum_boundary_is_valid()
    {
        await TestHost.ResetAndSeedUsuariosAsync(100);

        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new GetUsuariosQuery(Page: 1, PageSize: 100));

        result.Items.Count.ShouldBe(100);
        result.PageSize.ShouldBe(100);
        result.TotalPages.ShouldBe(1);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task GetUsuarios_via_api_invalid_page_returns_bad_request(int page)
    {
        var response = await _client.GetAsync($"/api/usuarios?page={page}&pageSize=5");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [TestCase(0)]
    [TestCase(101)]
    [TestCase(200)]
    public async Task GetUsuarios_via_api_invalid_page_size_returns_bad_request(int pageSize)
    {
        var response = await _client.GetAsync($"/api/usuarios?page=1&pageSize={pageSize}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [TestCase(0)]
    [TestCase(-5)]
    public async Task GetUsuarios_via_mediatr_invalid_page_throws_validation_exception(int page)
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Should.ThrowAsync<ValidationException>(() =>
            sender.Send(new GetUsuariosQuery(Page: page, PageSize: 5))
        );
    }

    [TestCase(0)]
    [TestCase(101)]
    public async Task GetUsuarios_via_mediatr_invalid_page_size_throws_validation_exception(
        int pageSize
    )
    {
        await using var scope = TestHost.WebFactory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Should.ThrowAsync<ValidationException>(() =>
            sender.Send(new GetUsuariosQuery(Page: 1, PageSize: pageSize))
        );
    }
}
