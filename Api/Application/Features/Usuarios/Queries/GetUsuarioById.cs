using Api.Domain.ValueObjects;
using Api.Infrastructure.Data.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.Queries;

public sealed record GetUsuarioByIdQuery(Guid Id) : IRequest<UsuarioDto?>;

public sealed class GetUsuarioByIdQueryValidator : AbstractValidator<GetUsuarioByIdQuery>
{
    public GetUsuarioByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

internal sealed class GetUsuarioByIdQueryHandler(AppDbContext db)
    : IRequestHandler<GetUsuarioByIdQuery, UsuarioDto?>
{
    public async Task<UsuarioDto?> Handle(
        GetUsuarioByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await db
            .Usuarios.AsNoTracking()
            .Where(u => u.Id == UsuarioId.From(request.Id))
            .Select(u => new UsuarioDto(u.Id.Value, u.Nombre, u.Apellido, u.Email.Value))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
