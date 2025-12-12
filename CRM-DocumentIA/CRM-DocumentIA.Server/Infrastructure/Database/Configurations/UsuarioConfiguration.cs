using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CRM_DocumentIA.Domain.ValueObjects;

namespace CRM_DocumentIA.Server.Infrastructure.Database.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            // Configuración para el Value Object Email
            builder.OwnsOne(u => u.Email, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.Property(e => e.Value)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);
            });

            builder.HasOne(u => u.Rol)
               .WithMany(r => r.Usuarios)
               .HasForeignKey(u => u.RolId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.RolId)
                .IsRequired()
                .HasDefaultValue(2);

            builder.Property(u => u.DobleFactorActivado)
                .HasDefaultValue(false);

            // ✅ Relación con Documentos
            builder.HasMany(u => u.Documentos)
                .WithOne(d => d.Usuario)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Mapeo para la tabla Usuarios
            builder.ToTable("Usuarios"); // Nombre explícito de la tabla
            
            builder.HasMany(u => u.DocumentosQueHeCompartido)
                .WithOne(dc => dc.UsuarioPropietario)
                .HasForeignKey(dc => dc.UsuarioPropietarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.DocumentosCompartidosConmigo)
                .WithOne(dc => dc.UsuarioCompartido)
                .HasForeignKey(dc => dc.UsuarioCompartidoId)
                .OnDelete(DeleteBehavior.Restrict);
                    }
    }
}