using System.Windows;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    public partial class ProfileView : System.Windows.Controls.UserControl
    {
        public ProfileViewModel ViewModel => (ProfileViewModel)DataContext;

        public ProfileView()
        {
            InitializeComponent();
        }

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
