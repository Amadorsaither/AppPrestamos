using System.Windows;
using Microsoft.EntityFrameworkCore;
using AppPrestamos.Data;
using AppPrestamos.Models;
using AppPrestamos.Services;
using AppPrestamos.Views;

namespace AppPrestamos
{
    /// <summary>
    /// Punto de entrada principal de la aplicación. Gestiona la migración de la base de datos,
    /// la inicialización del usuario administrador y la navegación entre ventanas de inicio de sesión y principal.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Usuario que ha iniciado sesión actualmente en la aplicación.
        /// </summary>
        public static Usuario? UsuarioActual { get; set; }

        /// <summary>
        /// Constructor que ejecuta las migraciones de Entity Framework y crea las tablas iniciales
        /// (Usuario y Auditoria) si no existen, luego siembra el usuario administrador por defecto.
        /// </summary>
        public App()
        {
            using (var db = new AppDbContext())
            {
                db.Database.Migrate();
                db.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS Usuario (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        NombreUsuario TEXT NOT NULL,
                        ContrasenaHash TEXT NOT NULL,
                        Rol TEXT NOT NULL DEFAULT 'Usuario',
                        Activo INTEGER NOT NULL DEFAULT 1,
                        FechaCreacion TEXT NOT NULL
                    )
                ");
                db.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS Auditoria (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UsuarioId INTEGER,
                        UsuarioNombre TEXT NOT NULL DEFAULT '',
                        Accion TEXT NOT NULL,
                        Entidad TEXT NOT NULL,
                        EntidadId INTEGER NOT NULL DEFAULT 0,
                        Detalle TEXT NOT NULL DEFAULT '',
                        Fecha TEXT NOT NULL
                    )
                ");
            }
            new AuthService().SeedAdminSiNoExiste();
        }

        /// <summary>
        /// Se ejecuta al iniciar la aplicación. Muestra la ventana de inicio de sesión.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            MostrarLogin();
        }

        /// <summary>
        /// Abre la ventana de inicio de sesión. Si el usuario inicia sesión correctamente,
        /// abre la ventana principal; si no, cierra la aplicación.
        /// </summary>
        public void MostrarLogin()
        {
            var login = new LoginWindow();
            if (login.ShowDialog() == true)
            {
                var main = new MainWindow();
                main.Show();

                main.Closed += (s, args) =>
                {
                    if (UsuarioActual == null)
                        MostrarLogin();
                    else
                        Shutdown();
                };
            }
            else
            {
                Shutdown();
            }
        }
    }
}
