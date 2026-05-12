using System.Windows.Controls;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Vista que permite gestionar el catálogo de clientes: alta, edición, búsqueda y eliminación.
    /// </summary>
    public partial class ClientesView : UserControl
    {
        public ClientesView()
        {
            InitializeComponent();
            DataContext = new ClientesViewModel();
        }
    }
}