using System.Windows.Input;
using AppPrestamos.Data;
using AppPrestamos.Models;
using AppPrestamos.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppPrestamos.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        [ObservableProperty]
        private string nombreUsuario = "";

        [ObservableProperty]
        private string inicial = "";

        [ObservableProperty]
        private string rol = "";

        [ObservableProperty]
        private DateTime fechaCreacion;

        [ObservableProperty]
        private string contrasenaActual = "";

        [ObservableProperty]
        private string nuevaContrasena = "";

        [ObservableProperty]
        private string confirmarContrasena = "";

        [ObservableProperty]
        private string mensaje = "";

        [ObservableProperty]
        private bool hayMensaje;

        [ObservableProperty]
        private bool mensajeExitoso;

        public ICommand CambiarContrasenaCommand { get; }

        public ProfileViewModel()
        {
            var usuario = App.UsuarioActual;
            if (usuario != null)
            {
                NombreUsuario = usuario.NombreUsuario;
                Inicial = usuario.NombreUsuario.Length > 0 ? usuario.NombreUsuario[..1].ToUpper() : "?";
                Rol = usuario.Rol;
                FechaCreacion = usuario.FechaCreacion;
            }

            CambiarContrasenaCommand = new RelayCommand(CambiarContrasena);
        }

        private void CambiarContrasena()
        {
            if (string.IsNullOrWhiteSpace(NuevaContrasena))
            {
                Mensaje = "Ingrese la nueva contraseña";
                MensajeExitoso = false;
                HayMensaje = true;
                return;
            }

            if (NuevaContrasena != ConfirmarContrasena)
            {
                Mensaje = "Las contraseñas no coinciden";
                MensajeExitoso = false;
                HayMensaje = true;
                return;
            }

            using var db = new AppDbContext();
            var usuario = db.Usuarios.Find(App.UsuarioActual!.Id);
            if (usuario == null)
            {
                Mensaje = "Usuario no encontrado";
                MensajeExitoso = false;
                HayMensaje = true;
                return;
            }

            var auth = new AuthService();
            if (auth.Login(NombreUsuario, ContrasenaActual) == null)
            {
                Mensaje = "La contraseña actual no es correcta";
                MensajeExitoso = false;
                HayMensaje = true;
                return;
            }

            var hash = HashContrasena(NuevaContrasena);
            usuario.ContrasenaHash = hash;
            db.SaveChanges();

            Mensaje = "Contraseña cambiada exitosamente";
            MensajeExitoso = true;
            HayMensaje = true;
            ContrasenaActual = "";
            NuevaContrasena = "";
            ConfirmarContrasena = "";
        }

        private static string HashContrasena(string contrasena)
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            byte[] salt = new byte[16];
            rng.GetBytes(salt);
            byte[] hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
                System.Text.Encoding.UTF8.GetBytes(contrasena),
                salt,
                100000,
                System.Security.Cryptography.HashAlgorithmName.SHA256,
                32);
            return System.Convert.ToBase64String(salt) + ":" + System.Convert.ToBase64String(hash);
        }
    }
}
