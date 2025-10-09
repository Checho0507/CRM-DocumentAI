// Infrastructure/Database/Configurations/InsightConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRM_DocumentIA.Server.Domain.Entities; // Ajuste de namespace

namespace CRM_DocumentIA.Server.Infrastructure.Database.Configurations
{
    public class InsightConfiguration : IEntityTypeConfiguration<Insight>
    {
        public void Configure(EntityTypeBuilder<Insight> builder)
        {
            // Nombre de la tabla
            builder.ToTable("Insights");

            // Clave primaria
            builder.HasKey(i => i.Id);

            // Propiedades básicas
            builder.Property(i => i.Tipo).IsRequired().HasMaxLength(50); // e.g., "Resumen", "AcciónClave"
            builder.Property(i => i.Contenido).IsRequired();
            builder.Property(i => i.GeneradoEn).IsRequired();

            // Relaciones

            // Relación obligatoria 1:N con Documento
            builder.HasOne(i => i.Documento)
                   .WithMany(d => d.Insights) // Asume que la entidad Documento tiene una ICollection<Insight> Insights
                   .HasForeignKey(i => i.DocumentoId)
                   .OnDelete(DeleteBehavior.Restrict) // Si el Documento se borra, los Insights asociados también
                   .IsRequired();

            // Relación opcional 1:N con Cliente (puede ser NULL)
            builder.HasOne(i => i.Cliente)
                   .WithMany(c => c.Insights) // Asume que la entidad Cliente tiene una ICollection<Insight> Insights
                   .HasForeignKey(i => i.ClienteId)
                   .OnDelete(DeleteBehavior.SetNull)
                   .IsRequired(false);
        }
    }
}