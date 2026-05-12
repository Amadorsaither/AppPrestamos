using System.Windows;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Ventana de inicio de sesión que autentica al usuario mediante
    /// nombre de usuario y contraseña antes de acceder al sistema.
    /// </summary>
    public partial class LoginWindow : Window
    {
        /// <summary>Obtiene el ViewModel asociado a la ventana.</summary>
        public LoginViewModel ViewModel => (LoginViewModel)DataContext;

        /// <summary>
        /// Inicializa la ventana de login y suscribe el evento de inicio de sesión exitoso
        /// para cerrar la ventana con resultado positivo.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();

            var vm = new LoginViewModel();
            vm.InicioSesionExitoso += (s, e) =>
            {
                DialogResult = true;
                Close();
            };
            DataContext = vm;
        }

        /// <summary>
        /// Maneja el clic del botón de inicio de sesión. Transfiere la contraseña
        /// desde el PasswordBox al ViewModel y ejecuta el comando de inicio de sesión.
        /// </summary>
        private void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Contrasena = TxtContrasena.Password;
            ViewModel.IniciarSesionCommand.Execute(null);
        }

        /// <summary>
        /// Alterna la visibilidad de la contraseña entre el modo oculto (PasswordBox)
        /// y el modo visible (TextBox), cambiando también el icono correspondiente.
        /// </summary>
        private void BtnTogglePass_Click(object sender, RoutedEventArgs e)
        {
            if (TxtContrasena.Visibility == Visibility.Visible)
            {
                TxtContrasenaVisible.Text = TxtContrasena.Password;
                TxtContrasena.Visibility = Visibility.Collapsed;
                TxtContrasenaVisible.Visibility = Visibility.Visible;
                TxtContrasenaVisible.Focus();
                IconEye.Visibility = Visibility.Collapsed;
                IconEyeOff.Visibility = Visibility.Visible;
            }
            else
            {
                TxtContrasena.Password = TxtContrasenaVisible.Text;
                TxtContrasena.Visibility = Visibility.Visible;
                TxtContrasenaVisible.Visibility = Visibility.Collapsed;
                TxtContrasena.Focus();
                IconEye.Visibility = Visibility.Visible;
                IconEyeOff.Visibility = Visibility.Collapsed;
            }
        }
    }
}
