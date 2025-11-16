using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nombre)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(r => r.Descripcion)
               .HasMaxLength(200);

        builder.ToTable("Roles");

        builder.HasData(
            new Rol("Admin", "Administrador del sistema") { Id = 1 },
            new Rol("Usuario", "Usuario estándar") { Id = 2 },
            new Rol("Analista", "Analista de documentos") { Id = 3 }
        );
    }
}
