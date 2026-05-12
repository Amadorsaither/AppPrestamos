using System.Windows.Input;
using AppPrestamos.Data;
using AppPrestamos.Models;
using AppPrestamos.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppPrestamos.ViewModels
{
    /// <summary>ViewModel para el perfil del usuario, muestra información y permite cambiar la contraseña</summary>
    public partial class ProfileViewModel : ObservableObject
    {
        /// <summary>Nombre de usuario del usuario autenticado</summary>
        [ObservableProperty]
        private string nombreUsuario = "";

        /// <summary>Inicial del nombre de usuario para mostrar en el avatar</summary>
        [ObservableProperty]
        private string inicial = "";

        /// <summary>Rol del usuario autenticado (Administrador, etc.)</summary>
        [ObservableProperty]
        private string rol = "";

        /// <summary>Fecha de creación de la cuenta del usuario</summary>
        [ObservableProperty]
        private DateTime fechaCreacion;

        /// <summary>Contraseña actual del usuario para verificar cambio</summary>
        [ObservableProperty]
        private string contrasenaActual = "";

        /// <summary>Nueva contraseña que se desea establecer</summary>
        [ObservableProperty]
        private string nuevaContrasena = "";

        /// <summary>Confirmación de la nueva contraseña para verificar coincidencia</summary>
        [ObservableProperty]
        private string confirmarContrasena = "";

        /// <summary>Mensaje informativo sobre el resultado del cambio de contraseña</summary>
        [ObservableProperty]
        private string mensaje = "";

        /// <summary>Indica si hay un mensaje informativo visible</summary>
        [ObservableProperty]
        private bool hayMensaje;

        /// <summary>Indica si el mensaje es de éxito (true) o de error (false)</summary>
        [ObservableProperty]
        private bool mensajeExitoso;

        /// <summary>Comando para cambiar la contraseña del usuario actual</summary>
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
