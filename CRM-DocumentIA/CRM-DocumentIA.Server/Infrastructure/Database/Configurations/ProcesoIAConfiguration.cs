using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM_DocumentIA.Server.Infrastructure.Database.Configurations
{
    public class ProcesoIAConfiguration : IEntityTypeConfiguration<ProcesoIA>
    {
        public void Configure(EntityTypeBuilder<ProcesoIA> builder)
        {
            builder.ToTable("ProcesosIA");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.TipoProcesamiento)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("analisis_documento");

            builder.Property(p => p.Estado)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("pendiente");

            builder.Property(p => p.Error)
                .HasMaxLength(1000);

            builder.Property(p => p.UrlServicio)
                .HasMaxLength(500);

            builder.Property(p => p.ResultadoJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.FechaInicio)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.TiempoProcesamientoSegundos)
                .IsRequired(false);

            // Relación con Documento - CAMBIAR A Restrict
            builder.HasOne(p => p.Documento)
                .WithMany(d => d.ProcesosIA)
                .HasForeignKey(p => p.DocumentoId)
                .OnDelete(DeleteBehavior.Restrict)  // ← CAMBIADO DE Cascade A Restrict
                .IsRequired();

            // Índices para mejor performance
            builder.HasIndex(p => p.DocumentoId);
            builder.HasIndex(p => p.Estado);
            builder.HasIndex(p => p.FechaInicio);
        }
    }
}