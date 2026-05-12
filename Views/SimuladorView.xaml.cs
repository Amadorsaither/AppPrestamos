using System.Windows.Controls;

namespace AppPrestamos.Views
{
    /// <summary>
    /// Vista del simulador de préstamos que permite calcular cuotas,
    /// intereses y tablas de amortización antes de formalizar un préstamo.
    /// </summary>
    public partial class SimuladorView : UserControl
    {
        public SimuladorView()
        {
            InitializeComponent();
            DataContext = new ViewModels.SimuladorViewModel();
        }
    }
}
