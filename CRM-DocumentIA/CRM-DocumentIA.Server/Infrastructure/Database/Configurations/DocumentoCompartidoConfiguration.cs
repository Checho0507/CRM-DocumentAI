// Infrastructure/Database/Configurations/DocumentoCompartidoConfiguration.cs
using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM_DocumentIA.Server.Infrastructure.Database.Configurations
{
    public class DocumentoCompartidoConfiguration : IEntityTypeConfiguration<DocumentoCompartido>
    {
        public void Configure(EntityTypeBuilder<DocumentoCompartido> builder)
        {
            builder.ToTable("DocumentosCompartidos");

            builder.HasKey(dc => dc.Id);

            builder.Property(dc => dc.Permiso)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("lectura");

            builder.Property(dc => dc.Mensaje)
                .HasMaxLength(500);

            builder.Property(dc => dc.FechaCompartido)
                .HasDefaultValueSql("GETDATE()");

            // Relación con Documento
            builder.HasOne(dc => dc.Documento)
                .WithMany(d => d.Compartidos)
                .HasForeignKey(dc => dc.DocumentoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con Usuario (Propietario)
            builder.HasOne(dc => dc.UsuarioPropietario)
                .WithMany(u => u.DocumentosQueHeCompartido)
                .HasForeignKey(dc => dc.UsuarioPropietarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con Usuario (Compartido)
            builder.HasOne(dc => dc.UsuarioCompartido)
                .WithMany(u => u.DocumentosCompartidosConmigo)
                .HasForeignKey(dc => dc.UsuarioCompartidoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice compuesto para evitar duplicados
            builder.HasIndex(dc => new { dc.DocumentoId, dc.UsuarioCompartidoId })
                .IsUnique();

            // Índices para búsquedas rápidas
            builder.HasIndex(dc => dc.UsuarioCompartidoId);
            builder.HasIndex(dc => dc.UsuarioPropietarioId);
            builder.HasIndex(dc => dc.FechaCompartido);
        }
    }
}