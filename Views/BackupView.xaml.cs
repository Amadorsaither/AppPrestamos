using System.Windows.Controls;

namespace AppPrestamos.Views
{
    public partial class BackupView : UserControl
    {
        public BackupView()
        {
            InitializeComponent();
            DataContext = new ViewModels.BackupViewModel();
        }
    }
}
