// Infrastructure/Database/Configurations/DocumentoConfiguration.cs

using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM_DocumentIA.Server.Infrastructure.Database.Configurations
{
    public class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
    {
        public void Configure(EntityTypeBuilder<Documento> builder)
        {
            // Nombre de la tabla
            builder.ToTable("Documentos");

            // Clave primaria
            builder.HasKey(d => d.Id);

            // Propiedades básicas
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

            // Campo binario (archivo físico en bytes)
            builder.Property(d => d.ArchivoDocumento)
                .HasColumnType("varbinary(max)")
                .IsRequired(false);

            // Metadatos JSON opcionales
            builder.Property(d => d.ArchivoMetadataJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            // Indicador de procesamiento
            builder.Property(d => d.Procesado)
                .HasDefaultValue(false);

            // ✅ Relación con Usuario
            builder.HasOne(d => d.Usuario)
                .WithMany(u => u.Documentos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }
}
