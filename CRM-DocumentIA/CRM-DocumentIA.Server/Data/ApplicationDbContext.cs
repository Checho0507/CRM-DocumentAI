using CRM_DocumentIA.server.Models;
using CRM_DocumentIA.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM_DocumentIA.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // DbSets para cada tabla
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Documento> Documentos { get; set; }
    public DbSet<ProcesoIA> ProcesosIA { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones adicionales si las necesitas
        modelBuilder.Entity<Documento>()
            .HasOne(d => d.Cliente)
            .WithMany(c => c.Documentos)
            .HasForeignKey(d => d.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProcesoIA>()
            .HasOne(p => p.Documento)
            .WithMany(d => d.ProcesosIA)
            .HasForeignKey(p => p.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}