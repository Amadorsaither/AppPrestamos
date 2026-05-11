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
    public partial class PrestamosViewModel : ObservableValidator
    {
        private readonly PrestamoService _prestamoService = new();

        [ObservableProperty]
        private ObservableCollection<Cliente> clientes = new();

        [ObservableProperty]
        private ObservableCollection<Prestamo> prestamos = new();

        [ObservableProperty]
        private Cliente? clienteSeleccionado;

        [ObservableProperty]
        private Prestamo? prestamoSeleccionado;

        [ObservableProperty]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        private decimal monto;

        [ObservableProperty]
        [Range(0, 100, ErrorMessage = "La tasa debe estar entre 0 y 100")]
        private decimal tasaInteres;

        [ObservableProperty]
        private TipoInteres tipoInteres = Enums.TipoInteres.Simple;

        [ObservableProperty]
        private FrecuenciaPago frecuenciaPago = Enums.FrecuenciaPago.Mensual;

        [ObservableProperty]
        [Range(1, 360, ErrorMessage = "El número de cuotas debe estar entre 1 y 360")]
        private int numeroCuotas = 12;

        [ObservableProperty]
        private DateTime fechaInicio = DateTime.Now;

        [ObservableProperty]
        [Range(0, 100, ErrorMessage = "La tasa de mora debe estar entre 0 y 100")]
        private decimal tasaMoraDiaria;

        [ObservableProperty]
        private string errorFormulario = string.Empty;

        public bool TieneError => !string.IsNullOrEmpty(ErrorFormulario);
        public Array TipoInteresValues { get; } = Enum.GetValues(typeof(Enums.TipoInteres));
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

        [RelayCommand]
        private void VerCuotas()
        {
            if (PrestamoSeleccionado is null) return;
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Cuotas"));
        }
    }
}
