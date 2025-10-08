using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplica automáticamente todas las configuraciones
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
