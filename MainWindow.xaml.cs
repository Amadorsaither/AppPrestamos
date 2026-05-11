using System.Windows;
using AppPrestamos.ViewModels;

namespace AppPrestamos
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            vm.SolicitarCierre += (s, e) => Close();
            DataContext = vm;
        }
    }
}
