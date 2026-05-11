using System.Windows.Controls;
using AppPrestamos.ViewModels;

namespace AppPrestamos.Views
{
    public partial class PagosView : UserControl
    {
        public PagosView()
        {
            InitializeComponent();
            DataContext = new PagosViewModel();
        }
    }
}
