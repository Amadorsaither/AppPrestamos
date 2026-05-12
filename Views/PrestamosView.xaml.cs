using System.Windows.Controls;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Vista que permite gestionar los préstamos: registro, cálculo de cuotas,
    /// visualización y seguimiento de préstamos activos.
    /// </summary>
    public partial class PrestamosView : UserControl
    {
        public PrestamosView()
        {
            InitializeComponent();
            DataContext = new PrestamosViewModel();
        }

        /// <summary>
        /// Maneja el cambio de selección en el DataGrid de préstamos.
        /// </summary>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
