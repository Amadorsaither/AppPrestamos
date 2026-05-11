using System.Windows;
using Microsoft.EntityFrameworkCore;
using AppPrestamos.Data;
using AppPrestamos.Models;
using AppPrestamos.Services;
using AppPrestamos.Views;

namespace AppPrestamos
{
    public partial class App : Application
    {
        public static Usuario? UsuarioActual { get; set; }

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

        protected override void OnStartup(StartupEventArgs e)
        {
            MostrarLogin();
        }

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
