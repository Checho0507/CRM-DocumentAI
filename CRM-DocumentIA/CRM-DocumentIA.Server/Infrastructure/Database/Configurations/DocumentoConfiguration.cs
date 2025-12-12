using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM_DocumentIA.Server.Infrastructure.Database.Configurations
{
    public class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
    {
        public void Configure(EntityTypeBuilder<Documento> builder)
        {
            builder.ToTable("Documentos");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.NombreArchivo)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.TipoDocumento)
                .HasMaxLength(50);

            builder.Property(d => d.RutaArchivo)
                .HasMaxLength(500);

            builder.Property(d => d.FechaSubida)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(d => d.TamañoArchivo)
                .IsRequired(false);

            builder.Property(d => d.EstadoProcesamiento)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("pendiente");

            builder.Property(d => d.UrlServicioIA)
                .HasMaxLength(1000);

            builder.Property(d => d.ErrorProcesamiento)
                .HasMaxLength(2000);

            // Configuración para el campo binario del archivo
            builder.Property(d => d.ArchivoDocumento)
                .HasColumnType("varbinary(max)")
                .IsRequired(false);

            // Configuración para el JSON de metadatos
            builder.Property(d => d.ArchivoMetadataJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            // Configuración para el resumen del documento
            builder.Property(d => d.ResumenDocumento)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            // Relación con Usuario
            builder.HasOne(d => d.Usuario)
                .WithMany(u => u.Documentos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasMany(d => d.Compartidos)
                .WithOne(dc => dc.Documento)
                .HasForeignKey(dc => dc.DocumentoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}