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
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Insight> Insights { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Documento> Documentos { get; set; } = null!;
        public DbSet<ProcesoIA> ProcesosIA { get; set; } = null!;
        public DbSet<TwoFA> TwoFA { get; set; } = null!;
        public DbSet<Rol> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ SOLO aplicar configuraciones desde archivos separados
            // NO configurar Email aquí, ya está en UsuarioConfiguration.cs
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}