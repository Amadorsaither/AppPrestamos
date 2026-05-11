using System.Windows.Controls;

namespace AppPrestamos.Views
{
    public partial class CalendarioView : UserControl
    {
        public CalendarioView()
        {
            InitializeComponent();
            DataContext = new ViewModels.CalendarioViewModel();
        }
    }
}
