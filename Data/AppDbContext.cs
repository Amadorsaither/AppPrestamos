using Microsoft.EntityFrameworkCore;
using AppPrestamos.Models;

namespace AppPrestamos.Data
{
    /// <summary>
    /// Contexto de base de datos de la aplicación. Expone las tablas del sistema de préstamos
    /// como conjuntos DbSet y configura las relaciones y restricciones mediante Fluent API.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>Clientes registrados en el sistema.</summary>
        public DbSet<Cliente> Clientes { get; set; }
        /// <summary>Préstamos otorgados a los clientes.</summary>
        public DbSet<Prestamo> Prestamos { get; set; }
        /// <summary>Cuotas generadas para cada préstamo.</summary>
        public DbSet<Cuota> Cuotas { get; set; }
        /// <summary>Pagos realizados sobre las cuotas.</summary>
        public DbSet<Pago> Pagos { get; set; }
        /// <summary>Usuarios del sistema.</summary>
        public DbSet<Usuario> Usuarios { get; set; }
        /// <summary>Registro de auditoría de acciones realizadas en el sistema.</summary>
        public DbSet<Auditoria> Auditorias { get; set; }

        /// <summary>
        /// Configura la conexión a la base de datos SQLite.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=prestamos.db");
        }

        /// <summary>
        /// Configura el modelo de datos usando Fluent API: tablas, relaciones, claves foráneas e índices.
        /// </summary>
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
