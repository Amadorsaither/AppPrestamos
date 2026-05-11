using System.Windows.Input;
using AppPrestamos.Models;
using AppPrestamos.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppPrestamos.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string nombreUsuario = "";

        [ObservableProperty]
        private string contrasena = "";

        [ObservableProperty]
        private string errorMensaje = "";

        [ObservableProperty]
        private bool hayError;

        public event EventHandler? InicioSesionExitoso;

        public ICommand IniciarSesionCommand { get; }

        public LoginViewModel()
        {
            IniciarSesionCommand = new RelayCommand(IniciarSesion);
        }

        private void IniciarSesion()
        {
            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                ErrorMensaje = "Ingrese su nombre de usuario";
                HayError = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(Contrasena))
            {
                ErrorMensaje = "Ingrese su contraseña";
                HayError = true;
                return;
            }

            var auth = new AuthService();
            var usuario = auth.Login(NombreUsuario, Contrasena);

            if (usuario == null)
            {
                ErrorMensaje = "Usuario o contraseña incorrectos";
                HayError = true;
                return;
            }

            App.UsuarioActual = usuario;
            HayError = false;
            InicioSesionExitoso?.Invoke(this, EventArgs.Empty);
        }
    }
}
