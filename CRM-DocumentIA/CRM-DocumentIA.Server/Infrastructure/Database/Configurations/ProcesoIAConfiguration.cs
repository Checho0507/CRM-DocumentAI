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

            builder.Property(p => p.TipoProceso)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Estado)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Resultado)
                .HasMaxLength(500);

            builder.HasOne(p => p.Documento)
                .WithMany(d => d.ProcesosIA)
                .HasForeignKey(p => p.DocumentoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
