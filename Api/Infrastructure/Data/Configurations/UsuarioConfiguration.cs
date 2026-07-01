using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Data.Configurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder
            .Property(u => u.Id)
            .HasConversion(id => id.Value, value => UsuarioId.From(value))
            .HasColumnName("Id");

        builder.Property(u => u.Nombre).HasMaxLength(100).IsRequired();

        builder.Property(u => u.Apellido).HasMaxLength(100).IsRequired();

        builder
            .Property(u => u.Email)
            .HasConversion(email => email.Value, value => Email.From(value))
            .HasMaxLength(256)
            .IsRequired();
    }
}
