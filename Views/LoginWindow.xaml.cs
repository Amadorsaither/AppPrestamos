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
    }
}
