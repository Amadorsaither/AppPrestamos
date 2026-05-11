using System.Security.Cryptography;
using System.Text;
using AppPrestamos.Data;
using AppPrestamos.Models;

namespace AppPrestamos.Services
{
    public class AuthService
    {
        public Usuario? Login(string nombreUsuario, string contrasena)
        {
            using var db = new AppDbContext();
            var usuario = db.Usuarios.FirstOrDefault(u =>
                u.NombreUsuario == nombreUsuario && u.Activo);

            if (usuario == null)
                return null;

            return VerificarContrasena(contrasena, usuario.ContrasenaHash) ? usuario : null;
        }

        public void CrearUsuario(string nombreUsuario, string contrasena, string rol = "Usuario")
        {
            using var db = new AppDbContext();
            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                ContrasenaHash = HashContrasena(contrasena),
                Rol = rol,
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            db.Usuarios.Add(usuario);
            db.SaveChanges();
        }

        public void SeedAdminSiNoExiste()
        {
            using var db = new AppDbContext();
            if (!db.Usuarios.Any())
            {
                db.Usuarios.Add(new Usuario
                {
                    NombreUsuario = "admin",
                    ContrasenaHash = HashContrasena("admin123"),
                    Rol = "Admin",
                    Activo = true,
                    FechaCreacion = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        private static string HashContrasena(string contrasena)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(contrasena),
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32);

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        private static bool VerificarContrasena(string contrasena, string almacenado)
        {
            var partes = almacenado.Split(':');
            if (partes.Length != 2)
                return false;

            byte[] salt = Convert.FromBase64String(partes[0]);
            byte[] hashAlmacenado = Convert.FromBase64String(partes[1]);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(contrasena),
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32);

            return CryptographicOperations.FixedTimeEquals(hashAlmacenado, hash);
        }
    }
}
