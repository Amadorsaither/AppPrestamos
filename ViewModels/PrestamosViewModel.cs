using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using AppPrestamos.Data;
using AppPrestamos.Enums;
using AppPrestamos.Models;
using AppPrestamos.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;

namespace AppPrestamos.ViewModels
{
    /// <summary>ViewModel para la gestión de préstamos (creación, cancelación y eliminación)</summary>
    public partial class PrestamosViewModel : ObservableValidator
    {
        private readonly PrestamoService _prestamoService = new();

        /// <summary>Colección observable de clientes disponibles para asignar un préstamo</summary>
        [ObservableProperty]
        private ObservableCollection<Cliente> clientes = new();

        /// <summary>Colección observable de préstamos registrados</summary>
        [ObservableProperty]
        private ObservableCollection<Prestamo> prestamos = new();

        /// <summary>Cliente seleccionado para el nuevo préstamo</summary>
        [ObservableProperty]
        private Cliente? clienteSeleccionado;

        /// <summary>Préstamo seleccionado en la lista</summary>
        [ObservableProperty]
        private Prestamo? prestamoSeleccionado;

        /// <summary>Monto del préstamo</summary>
        [ObservableProperty]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        private decimal monto;

        /// <summary>Tasa de interés del préstamo</summary>
        [ObservableProperty]
        [Range(0, 100, ErrorMessage = "La tasa debe estar entre 0 y 100")]
        private decimal tasaInteres;

        /// <summary>Tipo de interés (Simple o Compuesto)</summary>
        [ObservableProperty]
        private TipoInteres tipoInteres = Enums.TipoInteres.Simple;

        /// <summary>Frecuencia de pago (Mensual, Quincenal, etc.)</summary>
        [ObservableProperty]
        private FrecuenciaPago frecuenciaPago = Enums.FrecuenciaPago.Mensual;

        /// <summary>Número de cuotas del préstamo</summary>
        [ObservableProperty]
        [Range(1, 360, ErrorMessage = "El número de cuotas debe estar entre 1 y 360")]
        private int numeroCuotas = 12;

        /// <summary>Fecha de inicio del préstamo</summary>
        [ObservableProperty]
        private DateTime fechaInicio = DateTime.Now;

        /// <summary>Tasa de mora diaria por incumplimiento</summary>
        [ObservableProperty]
        [Range(0, 100, ErrorMessage = "La tasa de mora debe estar entre 0 y 100")]
        private decimal tasaMoraDiaria;

        /// <summary>Mensaje de error de validación del formulario</summary>
        [ObservableProperty]
        private string errorFormulario = string.Empty;

        /// <summary>Indica si hay errores de validación en el formulario</summary>
        public bool TieneError => !string.IsNullOrEmpty(ErrorFormulario);
        /// <summary>Valores del enumerado TipoInteres para el combo box</summary>
        public Array TipoInteresValues { get; } = Enum.GetValues(typeof(Enums.TipoInteres));
        /// <summary>Valores del enumerado FrecuenciaPago para el combo box</summary>
        public Array FrecuenciaPagoValues { get; } = Enum.GetValues(typeof(Enums.FrecuenciaPago));

        public PrestamosViewModel()
        {
            CargarClientes();
            CargarPrestamos();
        }

        partial void OnErrorFormularioChanged(string value) => OnPropertyChanged(nameof(TieneError));

        private void CargarClientes()
        {
            using var db = new AppDbContext();
            Clientes.Clear();
            foreach (var c in db.Clientes)
                Clientes.Add(c);
        }

        private void CargarPrestamos()
        {
            using var db = new AppDbContext();
            Prestamos.Clear();
            foreach (var p in db.Prestamos.Include("Cliente").OrderByDescending(p => p.FechaInicio))
                Prestamos.Add(p);
        }

        /// <summary>Crea un nuevo préstamo con los datos del formulario y genera sus cuotas</summary>
        [RelayCommand]
        private void CrearPrestamo()
        {
            ErrorFormulario = string.Empty;
            ValidateAllProperties();
            if (HasErrors) return;

            if (ClienteSeleccionado is null)
            {
                ErrorFormulario = "Seleccione un cliente.";
                return;
            }

            using var db = new AppDbContext();

            var prestamo = new Prestamo
            {
                ClienteId = ClienteSeleccionado.Id,
                Monto = Monto,
                TasaInteres = TasaInteres,
                TipoInteres = TipoInteres,
                FrecuenciaPago = FrecuenciaPago,
                NumeroCuotas = NumeroCuotas,
                FechaInicio = FechaInicio,
                TasaMoraDiaria = TasaMoraDiaria,
                Estado = EstadoPrestamo.Activo
            };

            var cuotas = _prestamoService.GenerarCuotas(prestamo);
            prestamo.Cuotas = cuotas;

            db.Prestamos.Add(prestamo);
            db.SaveChanges();

            new AuditService().Registrar("Crear", "Prestamo", prestamo.Id,
                $"Préstamo por ${prestamo.Monto:N2} a '{ClienteSeleccionado?.Nombre ?? ""}' (ID {prestamo.ClienteId}).");

            LimpiarFormulario();
            CargarPrestamos();

            MessageBox.Show("Préstamo creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LimpiarFormulario()
        {
            ClienteSeleccionado = null;
            Monto = 0;
            TasaInteres = 0;
            TipoInteres = Enums.TipoInteres.Simple;
            FrecuenciaPago = Enums.FrecuenciaPago.Mensual;
            NumeroCuotas = 12;
            FechaInicio = DateTime.Now;
            TasaMoraDiaria = 0;
            ErrorFormulario = string.Empty;
        }

        /// <summary>Cancela un préstamo activo marcándolo como pagado</summary>
        [RelayCommand]
        private void CancelarPrestamo()
        {
            if (PrestamoSeleccionado is null)
            {
                MessageBox.Show("Seleccione un préstamo de la lista.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (PrestamoSeleccionado.Estado != EstadoPrestamo.Activo)
            {
                MessageBox.Show("Solo se pueden cancelar préstamos activos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"¿Cancelar el préstamo #{PrestamoSeleccionado.Id}? Las cuotas pendientes quedarán anuladas.",
                "Confirmar cancelación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();
            var prestamo = db.Prestamos.Include(p => p.Cuotas).FirstOrDefault(p => p.Id == PrestamoSeleccionado.Id);
            if (prestamo is null) return;

            prestamo.Estado = EstadoPrestamo.Pagado;
            foreach (var cuota in prestamo.Cuotas.Where(c => c.Estado == EstadoCuota.Pendiente || c.Estado == EstadoCuota.Vencida))
            {
                cuota.Estado = EstadoCuota.Pagada;
                cuota.SaldoPendiente = 0;
            }

            db.SaveChanges();
            new AuditService().Registrar("Actualizar", "Prestamo", prestamo.Id,
                "Préstamo cancelado (marcado como Pagado).");
            CargarPrestamos();
            MessageBox.Show("Préstamo cancelado correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>Elimina un préstamo si no tiene pagos registrados</summary>
        [RelayCommand]
        private void EliminarPrestamo()
        {
            if (PrestamoSeleccionado is null)
            {
                MessageBox.Show("Seleccione un préstamo de la lista.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();
            var prestamo = db.Prestamos.Include(p => p.Cuotas).ThenInclude(c => c.Pagos)
                .FirstOrDefault(p => p.Id == PrestamoSeleccionado.Id);
            if (prestamo is null) return;

            if (prestamo.Cuotas.Any(c => c.Pagos.Any()))
            {
                MessageBox.Show("No se puede eliminar un préstamo que tiene pagos registrados. Cánclelo en su lugar.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                $"¿Eliminar el préstamo #{prestamo.Id}? Esta acción no se puede deshacer.",
                "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            db.Prestamos.Remove(prestamo);
            db.SaveChanges();
            new AuditService().Registrar("Eliminar", "Prestamo", prestamo.Id,
                $"Préstamo #{prestamo.Id} por ${prestamo.Monto:N2} eliminado.");
            PrestamoSeleccionado = null;
            CargarPrestamos();
            MessageBox.Show("Préstamo eliminado correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>Navega a la sección de cuotas del préstamo seleccionado</summary>
        [RelayCommand]
        private void VerCuotas()
        {
            if (PrestamoSeleccionado is null) return;
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Cuotas"));
        }
    }
}
