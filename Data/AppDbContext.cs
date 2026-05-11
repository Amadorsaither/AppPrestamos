using Microsoft.EntityFrameworkCore;
using AppPrestamos.Models;

namespace AppPrestamos.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }
        public DbSet<Cuota> Cuotas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=prestamos.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Prestamo>(entity =>
            {
                entity.ToTable("Prestamo");
                entity.HasOne(p => p.Cliente)
                    .WithMany(c => c.Prestamos)
                    .HasForeignKey(p => p.ClienteId);
            });

            modelBuilder.Entity<Cuota>(entity =>
            {
                entity.ToTable("Cuota");
                entity.HasOne(c => c.Prestamo)
                    .WithMany(p => p.Cuotas)
                    .HasForeignKey(c => c.PrestamoId);
            });

            modelBuilder.Entity<Pago>(entity =>
            {
                entity.ToTable("Pago");
                entity.HasOne(p => p.Cuota)
                    .WithMany(c => c.Pagos)
                    .HasForeignKey(p => p.CuotaId);
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasIndex(u => u.NombreUsuario).IsUnique();
            });

            modelBuilder.Entity<Auditoria>(entity =>
            {
                entity.ToTable("Auditoria");
            });
        }
    }
}
