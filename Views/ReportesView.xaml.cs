using System.Windows.Controls;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Vista de reportes y estadísticas del sistema, incluyendo
    /// gráficos y resúmenes de préstamos, cobros y morosidad.
    /// </summary>
    public partial class ReportesView : UserControl
    {
        public ReportesView()
        {
            InitializeComponent();
            DataContext = new ReportesViewModel();
        }
    }
}
