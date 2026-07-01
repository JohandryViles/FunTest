using Api.Application.Features.Usuarios.Queries;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Data.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.Commands;

public sealed record UpdateUsuarioRequest(string Nombre, string Apellido, string Email);

public sealed record UpdateUsuarioCommand(Guid Id, string Nombre, string Apellido, string Email)
    : IRequest<UsuarioDto?>;

public sealed class UpdateUsuarioCommandValidator : AbstractValidator<UpdateUsuarioCommand>
{
    public UpdateUsuarioCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.Apellido).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

internal sealed class UpdateUsuarioCommandHandler(AppDbContext db)
    : IRequestHandler<UpdateUsuarioCommand, UsuarioDto?>
{
    public async Task<UsuarioDto?> Handle(
        UpdateUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(
            u => u.Id == UsuarioId.From(request.Id),
            cancellationToken
        );

        if (usuario is null)
            return null;

        usuario.Nombre = request.Nombre.Trim();
        usuario.Apellido = request.Apellido.Trim();
        usuario.Email = Email.From(request.Email);

        await db.SaveChangesAsync(cancellationToken);

        return new UsuarioDto(
            usuario.Id.Value,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value
        );
    }
}
