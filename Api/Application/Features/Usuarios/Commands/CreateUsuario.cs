using Api.Application.Features.Usuarios.Queries;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Data.Persistence;
using FluentValidation;
using MediatR;

namespace Api.Application.Features.Usuarios.Commands;

public sealed record CreateUsuarioCommand(string Nombre, string Apellido, string Email)
    : IRequest<UsuarioDto>;

public sealed class CreateUsuarioCommandValidator : AbstractValidator<CreateUsuarioCommand>
{
    public CreateUsuarioCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty();
        RuleFor(x => x.Apellido).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

internal sealed class CreateUsuarioCommandHandler(AppDbContext db)
    : IRequestHandler<CreateUsuarioCommand, UsuarioDto>
{
    public async Task<UsuarioDto> Handle(
        CreateUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        var usuario = new Usuario
        {
            Id = UsuarioId.FromNewGuid(),
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Email = Email.From(request.Email),
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(cancellationToken);

        return new UsuarioDto(
            usuario.Id.Value,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value
        );
    }
}
