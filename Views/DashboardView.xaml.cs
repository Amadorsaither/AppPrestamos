using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Vista del panel principal (dashboard) que muestra resúmenes, indicadores
    /// y notificaciones del sistema.
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Maneja el evento de cierre del popup de notificaciones,
        /// sincronizando el estado con el ViewModel.
        /// </summary>
        private void NotificacionesPopup_Closed(object? sender, EventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
            {
                vm.IsNotificacionesOpen = false;
            }
        }
    }
}
