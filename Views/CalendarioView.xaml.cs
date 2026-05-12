using System.Windows.Controls;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Vista de calendario de pagos que muestra las fechas de vencimiento
    /// de las cuotas y permite visualizar los pagos programados.
    /// </summary>
    public partial class CalendarioView : UserControl
    {
        public CalendarioView()
        {
            InitializeComponent();
            DataContext = new ViewModels.CalendarioViewModel();
        }
    }
}
