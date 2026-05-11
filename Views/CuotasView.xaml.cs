using System.Windows.Controls;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    public partial class CuotasView : UserControl
    {
        public CuotasView()
        {
            InitializeComponent();
            DataContext = new CuotasViewModel();
        }
    }
}
