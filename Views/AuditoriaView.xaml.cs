using System.Windows.Controls;

namespace AppPrestamos.Views
{
    public partial class AuditoriaView : UserControl
    {
        public AuditoriaView()
        {
            InitializeComponent();
            DataContext = new ViewModels.AuditoriaViewModel();
        }
    }
}
