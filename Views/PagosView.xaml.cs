using System.Windows.Controls;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Vista que permite registrar y consultar los pagos realizados
    /// por los clientes sobre sus cuotas.
    /// </summary>
    public partial class PagosView : UserControl
    {
        public PagosView()
        {
            InitializeComponent();
            DataContext = new PagosViewModel();
        }
    }
}
