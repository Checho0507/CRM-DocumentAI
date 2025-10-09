using CRM_DocumentIA.Domain.ValueObjects;
using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CRM_DocumentIA.Server.Infrastructure.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSets para cada tabla
        // Agregamos el DbSet para Usuario y Insight
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Insight> Insights { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Documento> Documentos { get; set; } = null!;
        public DbSet<ProcesoIA> ProcesosIA { get; set; } = null!;
        public DbSet<TwoFA> TwoFA { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Conversión Email <-> string
            var emailConverter = new ValueConverter<Email, string>(
                email => email.Valor,          // modelo -> proveedor (DB)
                value => new Email(value)      // proveedor -> modelo (al leer)
            );

            // Comparador para tracking y snapshots
            var emailComparer = new ValueComparer<Email>(
                (l, r) => l != null && r != null && l.Valor == r.Valor,
                v => v.Valor.GetHashCode(),
                v => new Email(v.Valor)
            );

            modelBuilder.Entity<Usuario>(builder =>
            {
                builder.Property(u => u.Email)
                       .HasConversion(emailConverter)
                       .Metadata.SetValueComparer(emailComparer);
                // Si quieres, puedes configurar columna, longitud, etc:
                // .HasMaxLength(320).HasColumnName("Email");
            });

            // Aplica automáticamente todas las configuraciones
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
