using Vogen;

namespace Api.Domain.ValueObjects;

[ValueObject<Guid>]
public readonly partial struct UsuarioId
{
    private static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("El Id de usuario no puede estar vacío.");
}
