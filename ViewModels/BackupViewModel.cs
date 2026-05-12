using System.Windows;
using System.Windows.Input;
using AppPrestamos.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace AppPrestamos.ViewModels
{
    /// <summary>ViewModel para la sección de respaldo y restauración de la base de datos</summary>
    public partial class BackupViewModel : ObservableObject
    {
        /// <summary>Tamaño actual del archivo de base de datos</summary>
        [ObservableProperty]
        private string tamanoActual = "";

        /// <summary>Mensaje informativo sobre el resultado de la operación</summary>
        [ObservableProperty]
        private string mensaje = "";

        /// <summary>Indica si hay un mensaje informativo visible</summary>
        [ObservableProperty]
        private bool hayMensaje;

        /// <summary>Indica si el mensaje es de éxito (true) o de error (false)</summary>
        [ObservableProperty]
        private bool mensajeExitoso;

        /// <summary>Comando para crear un respaldo de la base de datos</summary>
        public ICommand CrearBackupCommand { get; }
        /// <summary>Comando para restaurar un respaldo de la base de datos</summary>
        public ICommand RestaurarCommand { get; }

        public BackupViewModel()
        {
            var svc = new BackupService();
            TamanoActual = svc.ObtenerTamanoDb();

            CrearBackupCommand = new RelayCommand(CrearBackup);
            RestaurarCommand = new RelayCommand(Restaurar);
        }

        private void CrearBackup()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Base de datos SQLite (*.db)|*.db|Todos los archivos (*.*)|*.*",
                FileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                var svc = new BackupService();
                svc.CrearBackup(dialog.FileName);
                MostrarMensaje("Respaldo creado exitosamente.", true);
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al crear respaldo: {ex.Message}", false);
            }
        }

        private void Restaurar()
        {
            var result = MessageBox.Show(
                "¿Restaurar respaldo? Se perderán todos los datos actuales. " +
                "La aplicación se cerrará después de la restauración.",
                "Confirmar restauración",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var dialog = new OpenFileDialog
            {
                Filter = "Base de datos SQLite (*.db)|*.db|Todos los archivos (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                var svc = new BackupService();
                svc.RestaurarBackup(dialog.FileName);
                MostrarMensaje("Respaldo restaurado. La aplicación se cerrará.", true);
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al restaurar: {ex.Message}", false);
            }
        }

        private void MostrarMensaje(string texto, bool exitoso)
        {
            Mensaje = texto;
            MensajeExitoso = exitoso;
            HayMensaje = true;
        }
    }
}
