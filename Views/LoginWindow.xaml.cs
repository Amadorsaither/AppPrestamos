using System.Windows;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    public partial class LoginWindow : Window
    {
        public LoginViewModel ViewModel => (LoginViewModel)DataContext;

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

        private void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Contrasena = TxtContrasena.Password;
            ViewModel.IniciarSesionCommand.Execute(null);
        }

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
