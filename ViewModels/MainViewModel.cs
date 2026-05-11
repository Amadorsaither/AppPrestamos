using System.Windows.Input;
using AppPrestamos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AppPrestamos.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private object? currentViewModel;

        [ObservableProperty]
        private string selectedSection = "Dashboard";

        public string UsuarioNombre => App.UsuarioActual?.NombreUsuario ?? "";
        public string UsuarioRol => App.UsuarioActual?.Rol ?? "";
        public string UsuarioInicial => UsuarioNombre.Length > 0 ? UsuarioNombre[..1].ToUpper() : "?";

        public event EventHandler? SolicitarCierre;

        public ICommand CerrarSesionCommand { get; }

        public MainViewModel()
        {
            CurrentViewModel = new DashboardViewModel();
            CerrarSesionCommand = new RelayCommand(CerrarSesion);

            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                switch (m.ViewName)
                {
                    case "Dashboard": NavigateToDashboard(); break;
                    case "Clientes": NavigateToClientes(); break;
                    case "Prestamos": NavigateToPrestamos(); break;
                    case "Cuotas": NavigateToCuotas(); break;
                    case "Pagos": NavigateToPagos(); break;
                    case "Reportes": NavigateToReportes(); break;
                    case "Backup": NavigateToBackup(); break;
                    case "Auditoria": NavigateToAuditoria(); break;
                    case "Simulador": NavigateToSimulador(); break;
                    case "Calendario": NavigateToCalendario(); break;
                    case "Profile": NavigateToProfile(); break;
                }
            });

            WeakReferenceMessenger.Default.Register<NavigateToPagoConCuotaMessage>(this, (r, m) =>
            {
                SelectedSection = "Pagos";
                CurrentViewModel = new PagosViewModel(m.CuotaId);
            });
        }

        private void CerrarSesion()
        {
            App.UsuarioActual = null;
            SolicitarCierre?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void NavigateToDashboard()
        {
            SelectedSection = "Dashboard";
            CurrentViewModel = new DashboardViewModel();
        }

        [RelayCommand]
        private void NavigateToClientes()
        {
            SelectedSection = "Clientes";
            CurrentViewModel = new ClientesViewModel();
        }

        [RelayCommand]
        private void NavigateToPrestamos()
        {
            SelectedSection = "Prestamos";
            CurrentViewModel = new PrestamosViewModel();
        }

        [RelayCommand]
        private void NavigateToCuotas()
        {
            SelectedSection = "Cuotas";
            CurrentViewModel = new CuotasViewModel();
        }

        [RelayCommand]
        private void NavigateToPagos()
        {
            SelectedSection = "Pagos";
            CurrentViewModel = new PagosViewModel();
        }

        [RelayCommand]
        private void NavigateToReportes()
        {
            SelectedSection = "Reportes";
            CurrentViewModel = new ReportesViewModel();
        }

        [RelayCommand]
        private void NavigateToBackup()
        {
            SelectedSection = "Backup";
            CurrentViewModel = new BackupViewModel();
        }

        [RelayCommand]
        private void NavigateToAuditoria()
        {
            SelectedSection = "Auditoria";
            CurrentViewModel = new AuditoriaViewModel();
        }

        [RelayCommand]
        private void NavigateToSimulador()
        {
            SelectedSection = "Simulador";
            CurrentViewModel = new SimuladorViewModel();
        }

        [RelayCommand]
        private void NavigateToCalendario()
        {
            SelectedSection = "Calendario";
            CurrentViewModel = new CalendarioViewModel();
        }

        [RelayCommand]
        private void NavigateToProfile()
        {
            SelectedSection = "Profile";
            CurrentViewModel = new ProfileViewModel();
        }
    }

    public class NavigationMessage
    {
        public string ViewName { get; }
        public NavigationMessage(string viewName) => ViewName = viewName;
    }

    public class NavigateToPagoConCuotaMessage
    {
        public int CuotaId { get; }
        public NavigateToPagoConCuotaMessage(int cuotaId) => CuotaId = cuotaId;
    }
}
