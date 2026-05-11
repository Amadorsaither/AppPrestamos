using System.Windows.Controls;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    public partial class PrestamosView : UserControl
    {
        public PrestamosView()
        {
            InitializeComponent();
            DataContext = new PrestamosViewModel();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
