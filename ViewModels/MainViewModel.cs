using System.Windows.Input;
using AppPrestamos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AppPrestamos.ViewModels
{
    /// <summary>ViewModel principal que maneja la navegación entre secciones de la aplicación</summary>
    public partial class MainViewModel : ObservableObject
    {
        /// <summary>ViewModel actual mostrado en el área de contenido principal</summary>
        [ObservableProperty]
        private object? currentViewModel;

        /// <summary>Nombre de la sección actualmente seleccionada en el menú de navegación</summary>
        [ObservableProperty]
        private string selectedSection = "Dashboard";

        /// <summary>Nombre del usuario autenticado actualmente</summary>
        public string UsuarioNombre => App.UsuarioActual?.NombreUsuario ?? "";
        /// <summary>Rol del usuario autenticado actualmente</summary>
        public string UsuarioRol => App.UsuarioActual?.Rol ?? "";
        /// <summary>Inicial del nombre del usuario para mostrar en la interfaz</summary>
        public string UsuarioInicial => UsuarioNombre.Length > 0 ? UsuarioNombre[..1].ToUpper() : "?";

        /// <summary>Evento que se dispara al solicitar el cierre de sesión</summary>
        public event EventHandler? SolicitarCierre;

        /// <summary>Comando para cerrar la sesión del usuario actual</summary>
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

        /// <summary>Navega a la sección de Dashboard</summary>
        [RelayCommand]
        private void NavigateToDashboard()
        {
            SelectedSection = "Dashboard";
            CurrentViewModel = new DashboardViewModel();
        }

        /// <summary>Navega a la sección de Clientes</summary>
        [RelayCommand]
        private void NavigateToClientes()
        {
            SelectedSection = "Clientes";
            CurrentViewModel = new ClientesViewModel();
        }

        /// <summary>Navega a la sección de Préstamos</summary>
        [RelayCommand]
        private void NavigateToPrestamos()
        {
            SelectedSection = "Prestamos";
            CurrentViewModel = new PrestamosViewModel();
        }

        /// <summary>Navega a la sección de Cuotas</summary>
        [RelayCommand]
        private void NavigateToCuotas()
        {
            SelectedSection = "Cuotas";
            CurrentViewModel = new CuotasViewModel();
        }

        /// <summary>Navega a la sección de Pagos</summary>
        [RelayCommand]
        private void NavigateToPagos()
        {
            SelectedSection = "Pagos";
            CurrentViewModel = new PagosViewModel();
        }

        /// <summary>Navega a la sección de Reportes</summary>
        [RelayCommand]
        private void NavigateToReportes()
        {
            SelectedSection = "Reportes";
            CurrentViewModel = new ReportesViewModel();
        }

        /// <summary>Navega a la sección de Backup</summary>
        [RelayCommand]
        private void NavigateToBackup()
        {
            SelectedSection = "Backup";
            CurrentViewModel = new BackupViewModel();
        }

        /// <summary>Navega a la sección de Auditoría</summary>
        [RelayCommand]
        private void NavigateToAuditoria()
        {
            SelectedSection = "Auditoria";
            CurrentViewModel = new AuditoriaViewModel();
        }

        /// <summary>Navega a la sección de Simulador</summary>
        [RelayCommand]
        private void NavigateToSimulador()
        {
            SelectedSection = "Simulador";
            CurrentViewModel = new SimuladorViewModel();
        }

        /// <summary>Navega a la sección de Calendario</summary>
        [RelayCommand]
        private void NavigateToCalendario()
        {
            SelectedSection = "Calendario";
            CurrentViewModel = new CalendarioViewModel();
        }

        /// <summary>Navega a la sección de Perfil</summary>
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
