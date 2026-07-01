using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public sealed class Usuario
{
    public required UsuarioId Id { get; init; }
    public required string Nombre { get; init; }
    public required string Apellido { get; init; }
    public required Email Email { get; init; }
}
