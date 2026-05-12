using System.Windows.Controls;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Vista que muestra el registro de auditoría con todas las acciones
    /// realizadas por los usuarios en el sistema.
    /// </summary>
    public partial class AuditoriaView : UserControl
    {
        public AuditoriaView()
        {
            InitializeComponent();
            DataContext = new ViewModels.AuditoriaViewModel();
        }
    }
}
