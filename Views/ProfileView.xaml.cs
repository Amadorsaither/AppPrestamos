using System.Windows;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Vista del perfil del usuario actual, que permite consultar sus datos
    /// y cambiar la contraseña de acceso al sistema.
    /// </summary>
    public partial class ProfileView : System.Windows.Controls.UserControl
    {
        /// <summary>Obtiene el ViewModel asociado a la vista.</summary>
        public ProfileViewModel ViewModel => (ProfileViewModel)DataContext;

        public ProfileView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Maneja el clic del botón de cambio de contraseña. Toma los valores
        /// de los campos de contraseña, ejecuta el comando y muestra el mensaje
        /// de resultado (éxito o error).
        /// </summary>
        private void BtnCambiarContrasena_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ContrasenaActual = TxtContrasenaActual.Password;
            ViewModel.NuevaContrasena = TxtNuevaContrasena.Password;
            ViewModel.ConfirmarContrasena = TxtConfirmarContrasena.Password;
            ViewModel.CambiarContrasenaCommand.Execute(null);

            TxtContrasenaActual.Password = "";
            TxtNuevaContrasena.Password = "";
            TxtConfirmarContrasena.Password = "";

            MensajeError.Visibility = Visibility.Collapsed;
            MensajeExito.Visibility = Visibility.Collapsed;

            if (ViewModel.MensajeExitoso)
            {
                MensajeExitoText.Text = ViewModel.Mensaje;
                MensajeExito.Visibility = Visibility.Visible;
            }
            else
            {
                MensajeErrorText.Text = ViewModel.Mensaje;
                MensajeError.Visibility = Visibility.Visible;
            }
        }
    }
}
