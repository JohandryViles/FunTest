using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public sealed class Usuario
{
    public required UsuarioId Id { get; init; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required Email Email { get; set; }
    public DateTime? DeletedAt { get; set; }

    internal Usuario() { }
}
