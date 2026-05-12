using System.Windows.Controls;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Vista que muestra las cuotas generadas para cada préstamo,
    /// permitiendo visualizar su estado (pendiente, pagada, vencida).
    /// </summary>
    public partial class CuotasView : UserControl
    {
        public CuotasView()
        {
            InitializeComponent();
            DataContext = new CuotasViewModel();
        }
    }
}
