using System.Windows.Controls;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Vista que permite realizar copias de seguridad y restaurar
    /// la base de datos del sistema.
    /// </summary>
    public partial class BackupView : UserControl
    {
        public BackupView()
        {
            InitializeComponent();
            DataContext = new ViewModels.BackupViewModel();
        }
    }
}
