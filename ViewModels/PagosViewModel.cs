using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using AppPrestamos.Data;
using AppPrestamos.Enums;
using AppPrestamos.Models;
using AppPrestamos.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace AppPrestamos.ViewModels
{
    public partial class PagosViewModel : ObservableValidator
    {
        [ObservableProperty]
        private ObservableCollection<Cuota> cuotasPendientes = new();

        [ObservableProperty]
        private ObservableCollection<Pago> pagos = new();

        [ObservableProperty]
        private Cuota? cuotaSeleccionada;

        [ObservableProperty]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        private decimal montoAPagar;

        [ObservableProperty]
        private string observacion = string.Empty;

        [ObservableProperty]
        private Pago? pagoSeleccionado;

        [ObservableProperty]
        private string errorFormulario = string.Empty;

        public bool TieneError => !string.IsNullOrEmpty(ErrorFormulario);

        public PagosViewModel() : this(null) { }

        public PagosViewModel(int? cuotaId)
        {
            CargarCuotasPendientes(cuotaId);
            CargarPagos();
        }

        partial void OnErrorFormularioChanged(string value) => OnPropertyChanged(nameof(TieneError));

        private void CargarCuotasPendientes(int? seleccionarId = null)
        {
            using var db = new AppDbContext();
            CuotasPendientes.Clear();
            Cuota? seleccionada = null;
            foreach (var c in db.Cuotas.Include("Prestamo.Cliente")
                         .Where(c => c.Estado == EstadoCuota.Pendiente || c.Estado == EstadoCuota.Vencida)
                         .OrderBy(c => c.FechaVencimiento))
            {
                CuotasPendientes.Add(c);
                if (seleccionarId.HasValue && c.Id == seleccionarId.Value)
                    seleccionada = c;
            }
            if (seleccionada is not null)
                CuotaSeleccionada = seleccionada;
        }

        private void CargarPagos()
        {
            using var db = new AppDbContext();
            Pagos.Clear();
            foreach (var p in db.Pagos.Include("Cuota.Prestamo.Cliente")
                         .OrderByDescending(p => p.FechaPago)
                         .Take(50))
                Pagos.Add(p);
        }

        partial void OnCuotaSeleccionadaChanged(Cuota? value)
        {
            if (value is not null)
                MontoAPagar = value.SaldoPendiente;
        }

        [RelayCommand]
        private void RegistrarPago()
        {
            ErrorFormulario = string.Empty;
            ValidateAllProperties();
            if (HasErrors) return;

            if (CuotaSeleccionada is null)
            {
                ErrorFormulario = "Seleccione una cuota.";
                return;
            }
            if (MontoAPagar <= 0)
            {
                ErrorFormulario = "Ingrese un monto válido.";
                return;
            }

            using var db = new AppDbContext();

            var cuota = db.Cuotas.Find(CuotaSeleccionada.Id);
            if (cuota is null) return;

            if (MontoAPagar > cuota.SaldoPendiente)
            {
                ErrorFormulario = $"El monto excede el saldo pendiente (${cuota.SaldoPendiente:N2}).";
                return;
            }

            var pago = new Pago
            {
                CuotaId = cuota.Id,
                FechaPago = DateTime.Now,
                MontoPagado = MontoAPagar,
                Observacion = Observacion
            };

            db.Pagos.Add(pago);

            cuota.SaldoPendiente -= MontoAPagar;
            if (cuota.SaldoPendiente <= 0)
            {
                cuota.SaldoPendiente = 0;
                cuota.Estado = EstadoCuota.Pagada;
            }

            db.SaveChanges();

            new AuditService().Registrar("Crear", "Pago", pago.Id,
                $"Pago de ${pago.MontoPagado:N2} a cuota #{cuota.NumeroCuota} (préstamo #{cuota.PrestamoId}).");

            MontoAPagar = 0;
            Observacion = string.Empty;
            CuotaSeleccionada = null;

            CargarCuotasPendientes();
            CargarPagos();

            MessageBox.Show("Pago registrado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void EliminarPago()
        {
            if (PagoSeleccionado is null)
            {
                MessageBox.Show("Seleccione un pago del historial.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"¿Eliminar el pago #{PagoSeleccionado.Id} por ${PagoSeleccionado.MontoPagado:N2}? Se restaurará el saldo de la cuota.",
                "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();

            var pago = db.Pagos.Include(p => p.Cuota).FirstOrDefault(p => p.Id == PagoSeleccionado.Id);
            if (pago is null) return;

            var cuota = pago.Cuota;
            cuota.SaldoPendiente += pago.MontoPagado;
            if (cuota.Estado == EstadoCuota.Pagada)
                cuota.Estado = EstadoCuota.Pendiente;

            db.Pagos.Remove(pago);
            db.SaveChanges();

            new AuditService().Registrar("Eliminar", "Pago", pago.Id,
                $"Pago de ${pago.MontoPagado:N2} eliminado de cuota #{cuota.NumeroCuota}.");

            PagoSeleccionado = null;
            CargarCuotasPendientes();
            CargarPagos();

            MessageBox.Show("Pago eliminado correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
