using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM_DocumentIA.Server.Infrastructure.Database.Configurations
{
    public class InsightConfiguration : IEntityTypeConfiguration<Insight>
    {
        public void Configure(EntityTypeBuilder<Insight> builder)
        {
            builder.ToTable("Insights");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.TipoInsight)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("general");

            builder.Property(i => i.Contenido)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(i => i.Confianza)
                .IsRequired(false);

            builder.Property(i => i.FechaGeneracion)
                .HasDefaultValueSql("GETDATE()");

            // Relación con Documento
            builder.HasOne(i => i.Documento)
                .WithMany(d => d.Insights)
                .HasForeignKey(i => i.DocumentoId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Relación con ProcesoIA (opcional)
            builder.HasOne(i => i.ProcesoIA)
                .WithMany()
                .HasForeignKey(i => i.ProcesoIAId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Índices para mejor performance
            builder.HasIndex(i => i.DocumentoId);
            builder.HasIndex(i => i.ProcesoIAId);
            builder.HasIndex(i => i.TipoInsight);
            builder.HasIndex(i => i.FechaGeneracion);
        }
    }
}