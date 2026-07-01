using Api.Domain.ValueObjects;
using Api.Infrastructure.Data.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.Commands;

public sealed record DeleteUsuarioCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteUsuarioCommandValidator : AbstractValidator<DeleteUsuarioCommand>
{
    public DeleteUsuarioCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

internal sealed class DeleteUsuarioCommandHandler(AppDbContext db)
    : IRequestHandler<DeleteUsuarioCommand, bool>
{
    public async Task<bool> Handle(
        DeleteUsuarioCommand request,
        CancellationToken cancellationToken
    )
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(
            u => u.Id == UsuarioId.From(request.Id),
            cancellationToken
        );

        if (usuario is null)
            return false;

        usuario.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
