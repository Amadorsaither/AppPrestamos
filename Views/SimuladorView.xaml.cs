using System.Windows.Controls;

namespace AppPrestamos.Views
{
    public partial class SimuladorView : UserControl
    {
        public SimuladorView()
        {
            InitializeComponent();
            DataContext = new ViewModels.SimuladorViewModel();
        }
    }
}
