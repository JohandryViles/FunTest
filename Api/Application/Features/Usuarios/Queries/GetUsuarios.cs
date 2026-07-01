using Api.Infrastructure.Data.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.Queries;

public sealed record GetUsuariosQuery(int Page = 1, int PageSize = 10)
    : IRequest<PaginatedUsuariosDto>;

public sealed record UsuarioDto(Guid Id, string Nombre, string Apellido, string Email);

public sealed record PaginatedUsuariosDto(
    IReadOnlyList<UsuarioDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public sealed class GetUsuariosQueryValidator : AbstractValidator<GetUsuariosQuery>
{
    public GetUsuariosQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

internal sealed class GetUsuariosQueryHandler(AppDbContext db)
    : IRequestHandler<GetUsuariosQuery, PaginatedUsuariosDto>
{
    public async Task<PaginatedUsuariosDto> Handle(
        GetUsuariosQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = db.Usuarios.AsNoTracking().OrderBy(u => u.Apellido).ThenBy(u => u.Nombre);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UsuarioDto(u.Id.Value, u.Nombre, u.Apellido, u.Email.Value))
            .ToListAsync(cancellationToken);

        var totalPages =
            totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PaginatedUsuariosDto(
            items,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages
        );
    }
}
