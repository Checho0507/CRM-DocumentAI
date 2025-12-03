using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM_DocumentIA.Server.Infrastructure.Database.Configurations
{
    public class InsightsHistoConfiguration : IEntityTypeConfiguration<InsightsHisto>
    {
        public void Configure(EntityTypeBuilder<InsightsHisto> builder)
        {
            // Nombre tabla
            builder.ToTable("insightsHisto");

            // PK
            builder.HasKey(x => x.Id);

            // Columnas
            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Date)
                .IsRequired();

            builder.Property(x => x.Question)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Answer)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.Usuario)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Chat)
               .WithMany(c => c.Insights)
               .HasForeignKey(i => i.ChatId)
               .OnDelete(DeleteBehavior.Cascade);

        }
    }
}