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

            // 🔹 Configuración para el campo binario del archivo
            builder.Property(d => d.ArchivoDocumento)
                .HasColumnType("varbinary(max)")
                .IsRequired(false);

            // 🔹 Configuración para el JSON de metadatos
            builder.Property(d => d.ArchivoMetadataJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            builder.HasOne(d => d.Cliente)
                .WithMany(c => c.Documentos)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }
}
