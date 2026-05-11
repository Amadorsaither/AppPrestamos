using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void NotificacionesPopup_Closed(object? sender, EventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
            {
                vm.IsNotificacionesOpen = false;
            }
        }
    }
}
