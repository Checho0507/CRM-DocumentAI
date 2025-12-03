using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM_DocumentIA.Server.Infrastructure.Database.Configurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.ToTable("Chats");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                .HasMaxLength(200);

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            // Relación 1:N con InsightsHisto
            builder.HasMany(c => c.Insights)
                .WithOne(i => i.Chat)
                .HasForeignKey(i => i.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
