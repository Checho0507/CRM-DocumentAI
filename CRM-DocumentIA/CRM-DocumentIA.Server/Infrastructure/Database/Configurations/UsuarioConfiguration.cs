// Infrastructure/Database/Configurations/UsuarioConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Domain.ValueObjects;

namespace CRM_DocumentIA.Infrastructure.Database.Configurations
{
    // Implementamos IEntityTypeConfiguration<Usuario>
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            // 1. Clave primaria
            builder.HasKey(u => u.Id);

            // 2. Value Converter para Email
            var emailConverter = new ValueConverter<Email, string>(
                v => v.Valor, // Conversión del objeto Email al string para la DB
                v => new Email(v) // Conversión del string de la DB al objeto Email
            );

            // Aplicamos el converter
            builder.Property(u => u.Email)
                  .HasConversion(emailConverter)
                  .IsRequired()
                  .HasMaxLength(256);

            // 3. Índice único para Email (importante para la autenticación)
            builder.HasIndex(u => u.Email).IsUnique();

            // 4. Mapeo para PasswordHash (debe ser largo)
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);

            // 5. Mapeo para Rol
            builder.Property(u => u.Rol).HasDefaultValue("usuario").IsRequired();

            // 6. Mapeo para la tabla Usuarios
            builder.ToTable("Usuarios"); // Nombre explícito de la tabla
        }
    }
}